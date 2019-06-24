using ApplicationCore.Entities;
using Google.Cloud.Datastore.V1;
using System;
using System.Linq;

namespace Infrastructure
{
    public static class Extensions
    {
        public static Key ToKey(this string payKey)
        {
            return new Key().WithElement(PaymentRepository.Kind, payKey);
        }

        public static string ToPayKey(this Key key)
        {
            return key.Path.First().Name;
        }

        public static Entity ToEntity(this PaymentDetails payment)
        {
            return new Entity()
            {
                Key = payment.PayKey.ToKey(),
                [nameof(PaymentDetails.InvoiceNo)] = payment.InvoiceNo,
                [nameof(PaymentDetails.Currency)] = payment.Currency,
                [nameof(PaymentDetails.AmountInternal)] = payment.AmountInternal,
                [nameof(PaymentDetails.Hashkey)] = payment.Hashkey,
                [nameof(PaymentDetails.Gateway)] = payment.Gateway,
                [nameof(PaymentDetails.TransactionStatus)] = (int)payment.TransactionStatus,
                [nameof(PaymentDetails.ArcadierTransactionStatus)] = (int)payment.ArcadierTransactionStatus,
                [nameof(PaymentDetails.CreatedAt)] = payment.CreatedAt
            };
        }

        public static PaymentDetails ToPayment(this Entity entity)
        {
            int transactionStatus = (int)entity[nameof(PaymentDetails.TransactionStatus)];
            int arcadierTransactionStatus = (int)entity[nameof(PaymentDetails.ArcadierTransactionStatus)];

            return new PaymentDetails
            {
                PayKey = entity.Key.ToPayKey(),
                InvoiceNo = (string)entity[nameof(PaymentDetails.InvoiceNo)],
                Currency = (string)entity[nameof(PaymentDetails.Currency)],
                AmountInternal = (string)entity[nameof(PaymentDetails.AmountInternal)],
                Hashkey = (string)entity[nameof(PaymentDetails.Hashkey)],
                Gateway = (string)entity[nameof(PaymentDetails.Gateway)],
                TransactionStatus = (TransactionStatus)transactionStatus,
                ArcadierTransactionStatus = (TransactionStatus)arcadierTransactionStatus,
                CreatedAt = (DateTime)entity[nameof(PaymentDetails.CreatedAt)]
            };
        }
    }
}
