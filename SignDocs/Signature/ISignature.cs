using System.Threading.Tasks;

namespace SignDocs.Signature
{
    public interface ISignature
    {
        string DocId { get; }
        string DocPath { get; }
        string Month { get; }
        string Type { get; }
        string User { get; }
        string Year { get; }

        Task CreateAsync();
    }
}