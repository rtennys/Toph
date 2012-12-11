using System;
using System.Collections.Generic;
using System.Linq;
using Toph.Common;

namespace Toph.Domain.Entities
{
    public class Invoice : EntityBase
    {
        protected Invoice()
        {
        }

        internal Invoice(UserProfile user, DateTimeOffset invoiceDate, string invoiceNumber)
        {
            UserProfile = user;
            InvoiceDate = invoiceDate;
            InvoiceNumber = invoiceNumber;
        }

        private readonly IList<InvoiceLineItem> _lineItems = new List<InvoiceLineItem>();

        public virtual UserProfile UserProfile { get; protected set; }
        public virtual DateTimeOffset InvoiceDate { get; protected set; }
        public virtual string InvoiceNumber { get; protected set; }
        public virtual InvoiceCustomer InvoiceCustomer { get; set; }

        public virtual IReadOnlyList<InvoiceLineItem> LineItems
        {
            get { return _lineItems.AsReadOnly(); }
        }

        public virtual double GetTotal()
        {
            return _lineItems.Sum(x => x.GetTotal());
        }
    }
}
