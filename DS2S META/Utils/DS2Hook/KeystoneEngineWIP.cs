using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.DS2Hook
{
    internal class KeystoneEngineWIP
    {
        // WIP
        //        private Engine Engine;
        //        private void AsmExecute(string asm)
        //        {
        //            //Assemble once to get the size
        //            EncodedData? bytes = Engine.Assemble(asm, (ulong)Process?.MainModule?.BaseAddress);

        //            KeystoneError error = Engine.GetLastKeystoneError();
        //            if (error != KeystoneError.KS_ERR_OK)
        //                throw new("Something went wrong during assembly. Code could not be assembled.");

        //            IntPtr insertPtr = GetPrefferedIntPtr(bytes.Buffer.Length, flProtect: Kernel32.PAGE_EXECUTE_READWRITE);

        //            //Reassemble with the location of the isertPtr to support relative instructions
        //            bytes = Engine.Assemble(asm, (ulong)insertPtr);
        //            error = Engine.GetLastKeystoneError();

        //            Kernel32.WriteBytes(Handle, insertPtr, bytes.Buffer);
        //#if DEBUG
        //            DebugPrintArray(bytes.Buffer);
        //#endif

        //            Execute(insertPtr);
        //            Free(insertPtr);
        //        }

        //#if DEBUG
        //        private static void DebugPrintArray(byte[] bytes)
        //        {
        //            Debug.WriteLine("");
        //            foreach (byte b in bytes)
        //            {
        //                Debug.Write($"{b:X2} ");
        //            }
        //            Debug.WriteLine("");
        //        }
        //#endif

    }
}
