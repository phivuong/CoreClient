namespace KTHub.Core.Client.Models
{
    public interface IRequestType<TEnumRequest>
    {
        TEnumRequest TypeName { get; set; }
    }
}