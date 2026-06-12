using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArasTrader.Application.Interfaces;
public interface ICustomerSyncService
{
    Task<int> SyncCustomersAsync(CancellationToken ct = default);
}

