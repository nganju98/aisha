using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
// using System.Windows.Forms;
using TextCopy;

namespace aisha;

public class Program
{
    private const string API_URL = "https://api.anthropic.com/v1/messages";
    private static readonly string CONFIG_FILE = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Aisha",
        "config.dat");

    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: aisha <natural language command>");
            Console.WriteLine("       aisha -dos <natural language command>");
            Console.WriteLine("       aisha config");
            return;
        }

        if (args[0].ToLower() == "config")
        {
            ConfigureApiKey();
            return;
        }

        string? apiKey = GetApiKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("API key not configured. Please run 'aisha config' to set up your API key.");
            return;
        }

        bool isDos = false;
        List<string> commandArgs = new List<string>(args);
        if (commandArgs[0].ToLower() == "-dos")
        {
            isDos = true;
            commandArgs.RemoveAt(0);
        }

        string commandType = isDos ? "DOS" : "PowerShell";
        string userInput = string.Join(" ", commandArgs);
        string prompt = $"Convert the following natural language command to a {commandType} command: '{userInput}'. Please provide only the {commandType} command without any explanation.";

        try
        {
            string command = await GetCommandAsync(prompt, apiKey);
            Console.WriteLine($"{commandType} command:");
            Console.WriteLine(command);
            CopyToClipboard(command);
            Console.WriteLine("Command copied to clipboard.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private static void ConfigureApiKey()
    {
        Console.Write("Enter your Claude API key: ");
        string? apiKey = Console.ReadLine();
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("API key cannot be empty.");
            return;
        }
        try
        {
            string? directoryPath = Path.GetDirectoryName(CONFIG_FILE);
            if (directoryPath == null)
            {
                Console.WriteLine("Error saving API key, invalid directory path:'" + directoryPath + "'.");
                return;
            }
            
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllText(CONFIG_FILE, EncryptString(apiKey));
            Console.WriteLine("API key saved successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving API key: {ex.Message}");
        }
    }

    private static string? GetApiKey()
    {
        try
        {
            if (File.Exists(CONFIG_FILE))
            {
                string encryptedKey = File.ReadAllText(CONFIG_FILE);
                return DecryptString(encryptedKey);
            } else {
                Console.WriteLine("API key not configured. Please run 'aisha config' to set up your API key.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving API key: {ex.Message}");
        }
        return null;
    }

    private static string EncryptString(string plainText)
    {
        byte[] encrypted;
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes("AishaSecretKey16"); // 16 bytes for AES-128
            aes.IV = new byte[16]; // Use a fixed IV for simplicity

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }
                encrypted = msEncrypt.ToArray();
            }
        }
        return Convert.ToBase64String(encrypted);
    }

    private static string DecryptString(string cipherText)
    {
        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes("AishaSecretKey16");
            aes.IV = new byte[16];

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
            {
                return srDecrypt.ReadToEnd();
            }
        }
    }

    private static async Task<string> GetCommandAsync(string prompt, string apiKey)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var requestBody = new
            {
                model = "claude-3-5-sonnet-20240620",
                max_tokens = 1000,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await client.PostAsync(API_URL, content);
        
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API request failed with status {response.StatusCode}: {errorContent}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);

            return jsonResponse.GetProperty("content")[0].GetProperty("text").GetString()!.Trim();
        }
    }
    private static bool IsRunningInPowerShell()
    {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PSExecutionPolicyPreference"));
        
    }
    
    private static void CopyToClipboard(string text)
    {
        ClipboardService.SetText(text);
    }
    
}