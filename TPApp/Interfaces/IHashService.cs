namespace TPApp.Interfaces
{
    public interface IHashService
    {
        string ComputeSha256(Stream stream);
        string ComputeSha256(byte[] bytes);
    }
}
