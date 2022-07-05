using System.Threading.Tasks;

namespace OnceMi.AspNetCore.MQ
{
    public interface ISubscribe
    {
        Task Excute();
    }
}
