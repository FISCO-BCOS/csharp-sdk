using System;

namespace FISCOBCOS.CSharpSdk.Lib
{
    /// <summary>
    /// Random Helper class
    /// </summary>
    public class RandomHelper
    {
        /// <summary>
        /// length
        /// </summary>
        private const int NonceSize = 24;

        /// <summary>
        /// get a random nonce
        /// </summary>
        /// <returns></returns>
        public static string GetRandomNonce()
        {
            return Convert.ToBase64String(GetRandomBytes(NonceSize));
        }

        public static string GetRandomNonce(int len)
        {
            return Convert.ToBase64String(GetRandomBytes(len));
        }

        public static byte[] GetRandomNonceByte()
        {
            return GetRandomBytes(NonceSize);
        }

        public static byte[] GetRandomBytes(int len)
        {
            var key = GetRandomizer(len, true, true, true, true);
            return System.Text.Encoding.UTF8.GetBytes(key);
        }

        #region generate a random string

        /// <summary>
        /// generate a random string
        /// </summary>
        /// <param name="intLength">length</param>
        /// <param name="booNumber">include number or not</param>
        /// <param name="booSign">include symbol or not</param>
        /// <param name="booSmallword">include lowercase letter or not</param>
        /// <param name="booBigword">include uppercase letter or not</param>
        /// <returns></returns>
        private static string GetRandomizer(int intLength, bool booNumber, bool booSign, bool booSmallword, bool booBigword)
        {
            //new an instance
            Random ranA = new Random();
            int intResultRound = 0;
            int intA = 0;
            string strB = "";

            while (intResultRound < intLength)
            {
                //generate a random number A, representing a type
                //1=number，2=symbol，3=small letter，4=capital letter

                intA = ranA.Next(1, 5);

                //If the random number A=1, run to generate the number
                //Generate a random number A, from 0 to 10
                //Turn the ramdon number A to a character (string)
                //after generating, bit+1, string accumulation, end loop

                if (intA == 1 && booNumber)
                {
                    intA = ranA.Next(0, 10);
                    strB = intA.ToString() + strB;
                    intResultRound = intResultRound + 1;
                    continue;
                }

                //If the random number A=2, run the generated symbol
                //Generate a random number A, representing the range
                //1：33-47 range，2：58-64 range，3：91-96 range，4：123-126 range

                if (intA == 2 && booSign == true)
                {
                    intA = ranA.Next(1, 5);

                    //If A=1
                    //Generate a random number A, representing Ascii code, 33-47
                    //Turn the random number into a charater string
                    //after generating, bit+1, string accumulation, end loop

                    if (intA == 1)
                    {
                        intA = ranA.Next(33, 48);
                        strB = ((char)intA).ToString() + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }

                    //If A=2
                    //Generate a random number A，Ascii code 58-64
                    //Turn the random number A into a charater string
                    //After generating, bit+1, string accumulation, end loop

                    if (intA == 2)
                    {
                        intA = ranA.Next(58, 65);
                        strB = ((char)intA).ToString() + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }

                    //If A=3
                    //Generate a random number A, Ascii code 91-96
                    //Turn the random number A into a charater string
                    //After generating, bit+1, string accumulation, end loop

                    if (intA == 3)
                    {
                        intA = ranA.Next(91, 97);
                        strB = ((char)intA).ToString() + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }

                    //If A=4
                    //Generate a random number A, Ascii code 123-126
                    //Turn the random number A into a charater string
                    //After generating, bit+1, string accumulation, end loop

                    if (intA == 4)
                    {
                        intA = ranA.Next(123, 127);
                        strB = ((char)intA).ToString() + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }
                }

                //If the random number A=3, run to generate lowercase letters
                //Gemerate a random number, 97-122
                //Turn the random number A into a character string
                //after generating, bit+1, string accumulation, end loop

                if (intA == 3 && booSmallword == true)
                {
                    intA = ranA.Next(97, 123);
                    strB = ((char)intA).ToString() + strB;
                    intResultRound = intResultRound + 1;
                    continue;
                }

                //If the random number A=4, run to generate uppercase letters
                //generate a random number, ranging from 65 to 90
                //Turn random number A into a character string
                //after generating, bit+1, string accumulation, end loop

                if (intA == 4 && booBigword == true)
                {
                    intA = ranA.Next(65, 89);
                    strB = ((char)intA).ToString() + strB;
                    intResultRound = intResultRound + 1;
                    continue;
                }
            }
            return strB;
        }

        #endregion generate a random string
    }
}