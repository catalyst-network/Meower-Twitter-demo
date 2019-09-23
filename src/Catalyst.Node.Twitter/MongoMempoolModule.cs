using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Catalyst.Abstractions.Mempool;
using Catalyst.Abstractions.Mempool.Repositories;
using Catalyst.Core.Lib.DAO;
using Catalyst.Core.Lib.Kernel;
using Catalyst.Core.Lib.Mempool.Documents;
using Catalyst.Core.Modules.Mempool;
using Catalyst.Core.Modules.Mempool.Repositories;
using SharpRepository.InMemoryRepository;
using SharpRepository.MongoDbRepository;
using SharpRepository.Repository;

namespace Catalyst.Node.POA.CE
{
    public class MongoMempoolModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new MongoDbRepository<TransactionBroadcastDao, string>())
                .As<IRepository<TransactionBroadcastDao, string>>()
                .SingleInstance();
            builder.RegisterType<MempoolDocumentRepository>().As<IMempoolRepository<TransactionBroadcastDao>>().SingleInstance();
            builder.RegisterType<Mempool>().As<IMempool<TransactionBroadcastDao>>().SingleInstance();
        }
    }
}
