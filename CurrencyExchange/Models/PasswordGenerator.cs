using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace CurrencyExchange.Models
{
    public interface IPasswordGenerator
    {
        string generateKey();
    }

    public class PasswordGenerator : IPasswordGenerator
    {
        int length = 8;

        const string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";


        StringBuilder sb = new StringBuilder();

        Random rnd = new Random();

        public string generateKey()
        {
            for (int i = 0; i < length; i++)
            {
                int index = rnd.Next(chars.Length);
                sb.Append(chars[index]);
            }

            return sb.ToString();
        }
    }
}