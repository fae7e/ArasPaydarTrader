using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArasTrader.Application.Interfaces;

public interface ITokenService
{
    Task<string> GetValidTokenAsync(CancellationToken ct = default);
    void InvalidateToken();
}
