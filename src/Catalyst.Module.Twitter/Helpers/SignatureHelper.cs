using Catalyst.Abstractions.Cryptography;
using Catalyst.Protocol.Cryptography;
using Catalyst.Protocol.Wire;
using Google.Protobuf;

namespace Catalyst.Module.Twitter.Helpers
{
    public static class SignatureHelper
    {
        public static Signature GenerateSignature(IWrapper cryptoWrapper, IPrivateKey privateKey,
            TransactionBroadcast transactionBroadcast, SigningContext signingContext)
        {
            var transactionWithoutSig = transactionBroadcast.Clone();
            transactionWithoutSig.Signature = null;

            var signature = cryptoWrapper.StdSign(privateKey, transactionWithoutSig.ToByteArray(),
                signingContext.ToByteArray());

            var sig = new Signature
            {
                RawBytes = ByteString.CopyFrom(signature.SignatureBytes), SigningContext = signingContext
            };
            return sig;
        }
    }
}