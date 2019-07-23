using System.Security.Cryptography;
using System.Text;
using System;

namespace MNF
{
    public class AESRef
    {
        private RijndaelManaged rijndaelCipher = null;
        private ICryptoTransform decryptTransform = null;
        private ICryptoTransform encryptTransform = null;

        public AESRef()
        {
            rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;
            rijndaelCipher.KeySize = 256;
            rijndaelCipher.BlockSize = 128;
        }

        public void setKey(string key, string iv)
        {
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            byte[] ivBytes = Encoding.ASCII.GetBytes(iv);

            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = ivBytes;

            decryptTransform = rijndaelCipher.CreateDecryptor();
            encryptTransform = rijndaelCipher.CreateEncryptor();
        }

        public byte[] decrypt(byte[] encryptedData, int offset, int count)
        {
            return decryptTransform.TransformFinalBlock(encryptedData, offset, count);
        }

        public byte[] decrypt(byte[] encryptedData)
        {
            return decrypt(encryptedData, 0, encryptedData.Length);
        }

        public string decrypt(string textToDecrypt)
        {
            byte[] encryptedData = Convert.FromBase64String(textToDecrypt);
            return Encoding.UTF8.GetString(decrypt(encryptedData));
        }

        public byte[] encrypt(byte[] textToEncrypt, int offset, int count)
        {
            return encryptTransform.TransformFinalBlock(textToEncrypt, offset, count);
        }

        public byte[] encrypt(byte[] toEncrypt)
        {
            return encrypt(toEncrypt, 0, toEncrypt.Length);
        }

        public string encrypt(string textToEncrypt)
        {
            byte[] plainText = Encoding.UTF8.GetBytes(textToEncrypt);
            return Convert.ToBase64String(encrypt(plainText));
        }
    }
    // It's implemented AesManaged.
    //public class AESManage
    //{
    //    AesManaged aesManager = null;
    //    ICryptoTransform encryptor = null;
    //    ICryptoTransform decryptor = null;

    //    public AESManage()
    //    {
    //        aesManager = new AesManaged();
    //    }

    //    public bool setKey(byte[] key, byte[] iv)
    //    {
    //        if (key.Length != 32)
    //            return false;

    //        if (iv.Length != 16)
    //            return false;

    //        aesManager.Key = key;
    //        aesManager.IV = iv;

    //        encryptor = aesManager.CreateEncryptor(aesManager.Key, aesManager.IV);
    //        decryptor = aesManager.CreateEncryptor(aesManager.Key, aesManager.IV);

    //        return true;
    //    }

    //    public byte[] decrypt(byte[] encryptedData, int offset, int count)
    //    {
    //        try
    //        {
    //            using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
    //            {
    //                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
    //                {
    //                    byte[] decrypt = new byte[msDecrypt.Length];
    //                    csDecrypt.Read(decrypt, 0, (int)msDecrypt.Length);
    //                    return decrypt;
    //                }
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            System.Console.WriteLine(e.Message);
    //            return null;
    //        }
    //    }

    //    public byte[] decrypt(byte[] encryptedData)
    //    {
    //        return decrypt(encryptedData, 0, encryptedData.Length);
    //    }

    //    public string decrypt(string textToDecrypt)
    //    {
    //        byte[] encryptedData = Convert.FromBase64String(textToDecrypt);
    //        return Encoding.UTF8.GetString(decrypt(encryptedData));
    //    }

    //    public byte[] encrypt(byte[] textToEncrypt, int offset, int count)
    //    {
    //        try
    //        {
    //            using (MemoryStream msEncrypt = new MemoryStream())
    //            {
    //                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
    //                {
    //                    csEncrypt.Write(textToEncrypt, offset, count);
    //                }
    //                return msEncrypt.ToArray();
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            System.Console.WriteLine(e.Message);
    //            return null;
    //        }
    //    }

    //    public byte[] encrypt(byte[] toEncrypt)
    //    {
    //        return encrypt(toEncrypt, 0, toEncrypt.Length);
    //    }

    //    public byte[] encrypt(string textToEncrypt)
    //    {
    //        try
    //        {
    //            using (MemoryStream msEncrypt = new MemoryStream())
    //            {
    //                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
    //                {
    //                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
    //                    {
    //                        swEncrypt.Write(textToEncrypt);
    //                    }
    //                    return msEncrypt.ToArray();
    //                }
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            System.Console.WriteLine(e.Message);
    //            return null;
    //        }
    //    }
    //}
}