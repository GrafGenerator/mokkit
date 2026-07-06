using System.Threading;
using System.Threading.Tasks;

namespace Mokkit.Example1.Common;

public interface IRequestHandler<TCommand, TResult>
{
    ValueTask<TResult> Handle(TCommand command, CancellationToken cancellationToken = default);
}