using System.Threading;
using System.Threading.Tasks;
using Protocol;

namespace Client
{
    public interface IWalletExtensionClient
    {
        Task<TransactionListExtention> GetTransactionsFromThisAsync(AccountPaginated accountPagination);

        Task<TransactionListExtention> GetTransactionsToThisAsync(AccountPaginated accountPagination);
    }
}