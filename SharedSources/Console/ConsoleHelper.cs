namespace Microshaoft
{
    using System;
    using System.Security;
    public static class ConsoleHelper
    {
        public static SecureString ReadMaskedSecureStringLine(string mask = "*")
        {
            var r = new SecureString();
            while (true)
            {
                var i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }

                if (i.Key == ConsoleKey.Backspace)
                {
                    if (r.Length > 0)
                    {
                        r.RemoveAt(r.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    r.AppendChar(i.KeyChar);
                    Console.Write(mask);
                }
            }
            r.MakeReadOnly();
            return r;
        }
    }
}
