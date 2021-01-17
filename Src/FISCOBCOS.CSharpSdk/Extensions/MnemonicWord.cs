using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FISCOBCOS.CSharpSdk
{
   public class MnemonicWord
    {

        public  string GetMnemonic(string passphrase)
        {

            Mnemonic mnemo = new Mnemonic(Wordlist.ChineseSimplified, WordCount.Twelve);
            ExtKey hdRoot = mnemo.DeriveExtKey(passphrase);
          
              StringBuilder sb = new StringBuilder();
            foreach (var u in mnemo.Words)
            {
                sb.Append(u + " ");
            }
            return sb.Remove(sb.Length-1,1).ToString();
         
        }

    }
}
