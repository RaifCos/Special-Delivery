using System;
using System.IO;
using System.Security.Cryptography;

public static class DataEncryption {

    // Encrypt Ciphertext into Plaintext
    public static string Encrypt(string plainText) {
        using Aes aes = Aes.Create();

        // NOTE: DataKeys is not included in Repo for Privacy.  
        aes.Key = DataKeys.GetKey();
        aes.IV = DataKeys.GetIV();

        using MemoryStream ms = new();
        using CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
        using StreamWriter sw = new(cs);

        sw.Write(plainText);
        sw.Close();

        return Convert.ToBase64String(ms.ToArray());
    }

    // Convert Ciphertext back into Plaintext
    public static string Decrypt(string cipherText) {
        byte[] buffer = Convert.FromBase64String(cipherText);

        using Aes aes = Aes.Create();
        aes.Key = DataKeys.GetKey();
        aes.IV = DataKeys.GetIV();

        using MemoryStream ms = new(buffer);
        using CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using StreamReader sr = new(cs);

        return sr.ReadToEnd();
    }
}
