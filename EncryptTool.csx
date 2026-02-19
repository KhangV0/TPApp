// Quick encrypt tool - run with: dotnet script EncryptTool.csx
// Or copy logic vào bất kỳ C# console app nào

var encryptKeyword = "TechPort@2026#SecretKey"; // phải khớp với AppSettings:EncryptKeyword trong appsettings.json
var plainText      = args.Length > 0 ? args[0] : "your_password_here";

var keyBytes   = System.Text.Encoding.UTF8.GetBytes(encryptKeyword);
var textBytes  = System.Text.Encoding.UTF8.GetBytes(plainText);

for (int i = 0; i < textBytes.Length; i++)
    textBytes[i] ^= keyBytes[i % keyBytes.Length];

var encrypted = Convert.ToBase64String(textBytes);

Console.WriteLine($"Plain:     {plainText}");
Console.WriteLine($"Encrypted: {encrypted}");
Console.WriteLine();
Console.WriteLine($"SQL: UPDATE SYS_PARAMETERS SET Val = '{encrypted}' WHERE Name = 'SMTP_PASSWORD';");
