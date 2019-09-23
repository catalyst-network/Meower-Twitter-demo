#region LICENSE

/**
* Copyright (c) 2019 Catalyst Network
*
* This file is part of Catalyst.Node <https://github.com/catalyst-network/Catalyst.Node>
*
* Catalyst.Node is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 2 of the License, or
* (at your option) any later version.
*
* Catalyst.Node is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with Catalyst.Node. If not, see <https://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Autofac;
using Catalyst.Abstractions;
using Catalyst.Abstractions.Cli;
using Catalyst.Abstractions.DAO;
using Catalyst.Abstractions.Types;
using Catalyst.Core.Lib;
using Catalyst.Core.Lib.Cli;
using Catalyst.Core.Lib.DAO;
using Catalyst.Core.Lib.DAO.Deltas;
using Catalyst.Core.Lib.Kernel;
using Catalyst.Core.Modules.Authentication;
using Catalyst.Core.Modules.Consensus;
using Catalyst.Core.Modules.Cryptography.BulletProofs;
using Catalyst.Core.Modules.Dfs;
using Catalyst.Core.Modules.KeySigner;
using Catalyst.Core.Modules.Keystore;
using Catalyst.Core.Modules.Ledger;
using Catalyst.Core.Modules.P2P.Discovery.Hastings;
using Catalyst.Core.Modules.Rpc.Server;
using Catalyst.Core.Modules.Web3;
using Catalyst.Module.Twitter.Config;
using Catalyst.Module.Twitter.Interfaces;
using Catalyst.Module.TwitterLikes.Interfaces;
using Catalyst.Module.TwitterLikes.Models;
using Catalyst.Module.TwitterLikes.Repository;
using Catalyst.Module.TwitterLikes.Services;
using Catalyst.Modules.POA.Consensus;
using Catalyst.Modules.POA.P2P;
using Catalyst.Protocol.Network;
using CommandLine;
using SharpRepository.MongoDbRepository;
using SharpRepository.Repository;

namespace Catalyst.Node.POA.CE
{
    internal class Options
    {
        [Option("ipfs-password", HelpText = "The password for IPFS.  Defaults to prompting for the password.")]
        public string IpfsPassword { get; set; }

        [Option("ssl-cert-password", HelpText = "The password for ssl cert.  Defaults to prompting for the password.")]
        public string SslCertPassword { get; set; }

        [Option("node-password", HelpText = "The password for the node.  Defaults to prompting for the password.")]
        public string NodePassword { get; set; }

        [Option('o', "overwrite-config", HelpText = "Overwrite the data directory configs.")]
        public bool OverwriteConfig { get; set; }

        [Option("network-file", HelpText = "The name of the network file")]
        public string OverrideNetworkFile { get; set; }

        [Option('t', "tlu", HelpText = "Twitter Likes Url")]
        public string TwitterLikeUrl { get; set; }
    }

    internal static class Program
    {
        private static readonly Kernel Kernel;

        static Program()
        {
            Kernel = Kernel.Initramfs();

            AppDomain.CurrentDomain.UnhandledException += Kernel.LogUnhandledException;
            AppDomain.CurrentDomain.ProcessExit += Kernel.CurrentDomain_ProcessExit;
        }

        /// <summary>
        ///     For ref what passing custom boot logic looks like, this is the same as Kernel.StartNode()
        /// </summary>
        /// <param name="kernel"></param>
        /// <returns></returns>
        private static void CustomBootLogic(Kernel kernel)
        {
            var mappers = new IMapperInitializer[]
            {
                new ProtocolMessageDao(),
                new ConfidentialEntryDao(),
                new CandidateDeltaBroadcastDao(),
                new ProtocolErrorMessageSignedDao(),
                new PeerIdDao(),
                new SigningContextDao(),
                new DeltaDao(),
                new CandidateDeltaBroadcastDao(),
                new DeltaDfsHashBroadcastDao(),
                new FavouriteDeltaBroadcastDao(),
                new CoinbaseEntryDao(),
                new PublicEntryDao(),
                new ConfidentialEntryDao(),
                new TransactionBroadcastDao(),
                new RangeProofDao(),
                new ContractEntryDao(),
                new SignatureDao(),
                new BaseEntryDao()
            };

            var map = new MapperProvider(mappers);
            map.Start();

            // core modules
            Kernel.ContainerBuilder.RegisterType<CatalystNodePoa>().As<ICatalystNode>();
            Kernel.ContainerBuilder.RegisterType<ConsoleUserOutput>().As<IUserOutput>();
            Kernel.ContainerBuilder.RegisterType<ConsoleUserInput>().As<IUserInput>();

            // core modules
            Kernel.ContainerBuilder.RegisterModule(new CoreLibProvider());

            Kernel.ContainerBuilder.RegisterModule(new MongoMempoolModule());

            Kernel.ContainerBuilder.RegisterModule(new ConsensusModule());
            Kernel.ContainerBuilder.RegisterModule(new LedgerModule());
            Kernel.ContainerBuilder.RegisterModule(new DiscoveryHastingModule());
            Kernel.ContainerBuilder.RegisterModule(new RpcServerModule());
            Kernel.ContainerBuilder.RegisterModule(new BulletProofsModule());
            Kernel.ContainerBuilder.RegisterModule(new KeystoreModule());
            Kernel.ContainerBuilder.RegisterModule(new KeySignerModule());
            Kernel.ContainerBuilder.RegisterModule(new RpcServerModule());
            Kernel.ContainerBuilder.RegisterModule(new DfsModule());
            Kernel.ContainerBuilder.RegisterModule(new ConsensusModule());
            Kernel.ContainerBuilder.RegisterModule(new BulletProofsModule());
            Kernel.ContainerBuilder.RegisterModule(new AuthenticationModule());

            // node modules
            kernel.ContainerBuilder.RegisterModule(new PoaConsensusModule());
            kernel.ContainerBuilder.RegisterModule(new PoaP2PModule());

            Kernel.ContainerBuilder.Register(c => new MongoDbRepository<TweetLike, string>())
                .As<IRepository<TweetLike, string>>()
                .SingleInstance();
            Kernel.ContainerBuilder.RegisterType<TweetLikeRepository>().As<ITweetLikeRepository>().SingleInstance();
            Kernel.ContainerBuilder.RegisterType<TweetLikeService>().As<ITweetLikeService>().SingleInstance();

            kernel.ContainerBuilder.RegisterModule(new ApiModule("http://*:5006",
                new List<string>
                    {"Catalyst.Core.Modules.Web3", "Catalyst.Module.Twitter", "Catalyst.Module.TwitterLikes"}));

            kernel.StartContainer();
            kernel.Instance.Resolve<ICatalystNode>()
                .RunAsync(new CancellationToken())
                .Wait();
        }

        public static int Main(string[] args)
        {
            // Parse the arguments.
            Parser.Default
                .ParseArguments<Options>(args)
                .WithParsed(Run);

            return Environment.ExitCode;
        }

        private static void Run(Options options)
        {
            if (string.IsNullOrEmpty(options.TwitterLikeUrl))
            {
                options.TwitterLikeUrl = "http://127.0.0.1:5006";
            }

            Kernel.ContainerBuilder.RegisterType<TwitterModuleConfig>().As<ITwitterModuleConfig>()
                .WithProperty("TwitterLikesApiUrl", options.TwitterLikeUrl);

            Kernel.Logger.Information("Catalyst.Node started with process id {0}",
                Process.GetCurrentProcess().Id.ToString());

            try
            {
                Kernel
                    .WithDataDirectory()
                    .WithNetworksConfigFile(NetworkType.Devnet, options.OverrideNetworkFile)
                    .WithConfigurationFile(PoaConstants.P2PMessageHandlerConfigFile)
                    .WithConfigurationFile(PoaConstants.RpcMessageHandlerConfigFile)
                    .WithSerilogConfigFile()
                    .WithConfigCopier(new PoaConfigCopier())
                    .WithPersistenceConfiguration()
                    .BuildKernel(options.OverwriteConfig)
                    .WithPassword(PasswordRegistryTypes.DefaultNodePassword, options.NodePassword)
                    .WithPassword(PasswordRegistryTypes.IpfsPassword, options.IpfsPassword)
                    .WithPassword(PasswordRegistryTypes.CertificatePassword, options.SslCertPassword)
                    .StartCustom(CustomBootLogic);

                Environment.ExitCode = 0;
            }
            catch (Exception e)
            {
                Kernel.Logger.Fatal(e, "Catalyst.Node stopped unexpectedly");
                Environment.ExitCode = 1;
            }
        }
    }
}