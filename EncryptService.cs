using System.Text;

namespace CastleCoreTest
{
    public interface IEncryptService
    {
        public string Encrypt(string str);
    }
    public class EncryptService : IEncryptService
    {
        public string Encrypt(string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }
    }
}
