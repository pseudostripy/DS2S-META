using DS2S_META.Utils.DS2Hook;
using DS2S_META.Utils.Offsets.CodeLocators;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.OffsetClasses
{
    public class REDataUnpacker
    {
        // Fields/Properties
        private readonly DS2SHook Hook;
        private readonly DS2VER Ver;

        // Output properties
        public List<string> InfoErrors = new();
        public Dictionary<string, PHPointer> PHPDict { get; set; } = new(); // final resolved output ptrs
        public Dictionary<string, PHLeaf?> Leaves { get; set; } = new();    // final resolved output leaves
        public Dictionary<string, Dictionary<string, PHLeaf?>> LeafGroups { get; set; } = new(); // final resolved output leafGroups

        // Internal properties
        private Dictionary<string, ChildPointerCL> LimboDictCL { get; set; } = new();
        private Dictionary<string, ChildPointerCL> ChildPtrDefnsDict { get; set; } = new();
        private Dictionary<string, CodeLocator> PtrDefnsDict { get; set; } = new();
        private Dictionary<string, AobCodeLocator> AobPtrDefnsDict { get; set; } = new();

        // Constructor
        public REDataUnpacker(DS2SHook hook, DS2VER ver)
        {
            Hook = hook;
            Ver = ver;
            SetupPointers();
        }

        public void SetupPointers()
        {
            UnpackIntoDictionaries();
            DoAoBPtrs();
            DoChildPtrs();
            DoLeaves();
            DoLeafGroups();



            if (InfoErrors.Count > 0)
                MetaExceptionStaticHandler.Raise("Some issue in pointer setup, please debug here"); // we're not on dispatcher!
        }

        // Core
        private void UnpackIntoDictionaries()
        {
            PtrDefnsDict = DS2REData.PointerDefns.Where(x => x.HasVerLocator(Ver))
                                       .ToDictionary(x => x.Identifier, x => x.GetVerLocator(Ver));
            AobPtrDefnsDict = PtrDefnsDict.Where(kvp => kvp.Value is AobCodeLocator)
                                            .ToDictionary(kvp => kvp.Key, kvp => (AobCodeLocator)kvp.Value);
            ChildPtrDefnsDict = PtrDefnsDict.Where(kvp => kvp.Value is not AobCodeLocator)
                                      .ToDictionary(kvp => kvp.Key, kvp => (ChildPointerCL)kvp.Value);
        }
        private void DoAoBPtrs()
        {
            foreach (var kvp in AobPtrDefnsDict)
            {
                var php = kvp.Value.Init(Hook);
                PHPDict[kvp.Key] = php;
            }
            Hook.RescanAOB(); // resolve AbsAob things in one go

            // TODO:  HANDLE OLD BBJ MOD STUFF

            RemoveUnresolvedAoBPtrs();
        }
        private void DoChildPtrs()
        {
            foreach (var kvp in ChildPtrDefnsDict)
            {
                var childid = kvp.Key;
                var childCL = kvp.Value;
                var parentid = childCL.ParentPtrId;

                // Handle missing parent:
                if (!PHPDict.ContainsKey(parentid))
                    HandleUnresolvedParentPtr(childid, childCL, parentid);
                else
                    HandleNominalChildPtr(childid, childCL, PHPDict[parentid]);
            }
        }
        private void DoLeaves()
        {
            foreach (var leafdef in DS2REData.LeafDefns)
                Leaves.Add(leafdef.Id, ResolveLeafDefn(leafdef));

        }
        private void DoLeafGroups()
        {
            foreach (var grp in DS2REData.LeafGroupDefns)
                LeafGroups[grp.GroupId] = ResolveLLG(grp);
        }

        // Helper
        private PHLeaf? ResolveLeafDefn(LeafDefn leafdef)
        {
            var leafcl = leafdef.GetLocator(Ver);
            if (leafcl == null) return null;

            PHPDict.TryGetValue(leafcl.ParentPtrId, out var parentptr);
            if (parentptr == null) return null;

            return leafcl.InitFromParent(Hook, parentptr);
        }
        private Dictionary<string, PHLeaf?> ResolveLLG(LeafLocatorGroup grp)
        {
            Dictionary<string, PHLeaf?> leaves = new();
            
            foreach (var leafDefn in grp.Leaves)
                leaves[leafDefn.Id] = ResolveLeafDefn(leafDefn); // very fractionally slower, but more readable
            return leaves;
        }
        private void RemoveUnresolvedAoBPtrs()
        {
            // Remove unresolved AoB ptrs
            var phpUnresolvedAobs = PHPDict.Where(kvp => kvp.Value.Resolve() == IntPtr.Zero);
            foreach (var kvp in phpUnresolvedAobs)
                PHPDict.Remove(kvp.Key);
        }
        private int AddInfoError(string msg)
        {
            InfoErrors.Add(msg);
            return -1;
        }
        private int HandleUnresolvedParentPtr(string childid, ChildPointerCL childCL, string parentId)
        {
            if (AobPtrDefnsDict.ContainsKey(parentId))
                return AddInfoError($"ChildPtr {childid} unresolvable: parent AoBptr {parentId} scan failed."); // parent is AoBCL
            if (!ChildPtrDefnsDict.ContainsKey(parentId))
               return AddInfoError($"ChildPtr {childid} unresolvable: parent AoBptr {parentId} not defined anywhere."); // Parent is undefined

            // parent not yet handled
            LimboDictCL.Add(childid, childCL);
            return 0;
        }
        private int HandleNominalChildPtr(string childid, ChildPointerCL childCL, PHPointer resolvedParentPtr)
        {
            // Handle nominal (parent exists and is resolved)
            var toAddPhP = childCL.InitFromParent(Hook, resolvedParentPtr);
            //if (toAddPhP.Resolve() == IntPtr.Zero)
            //    return AddInfoError($"ChildPtr {childid} unresolvable: offsets cannot be followed from the well-defined parent"); // Parent is undefined
            RecursiveAddToDict(childid, toAddPhP);
            return 1;
        }

        private void RecursiveAddToDict(string toAddId, PHPointer toAdd)
        {
            // We still need to add the children recursively. This is essentially fixing up the
            // offset order, since it can be defined somewhat arbitrarily in DS2REData

            // add it
            PHPDict[toAddId] = toAdd;

            // Handle children recurssively
            var limboToHandle = LimboDictCL.Where(kvp => kvp.Value.ParentPtrId == toAddId).ToList();
            foreach (var kvp in limboToHandle)
            {
                // parent guaranteed to exist and be resolved otherwise we wouldn't get here
                var limboChildId = kvp.Key;
                var limboChildCL = kvp.Value;
                var limboPHP = limboChildCL.InitFromParent(Hook, toAdd);
                //if (limboPHP.Resolve() == IntPtr.Zero)
                //{
                //    InfoErrors.Add($"ChildPtr {limboPHP} unresolvable: Limbo child offsets cannot be followed from the well-defined parent"); // Parent is undefined
                //    continue;
                //}
                LimboDictCL.Remove(limboChildId); // handled
                RecursiveAddToDict(limboChildId, limboPHP);
            }
        }
    }
}
