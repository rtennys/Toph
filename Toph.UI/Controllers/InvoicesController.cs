﻿using System;
using System.Linq;
using System.Web.Mvc;
using Toph.Common;
using Toph.Common.DataAccess;
using Toph.Domain.Entities;
using Toph.UI.Models;

namespace Toph.UI.Controllers
{
    [Authorize]
    public class InvoicesController : AppController
    {
        public InvoicesController(IRepository repository, IUnitOfWork uow)
        {
            _repository = repository;
            _uow = uow;
        }

        private readonly IRepository _repository;
        private readonly IUnitOfWork _uow;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Load()
        {
            var model = _repository
                .Find<Invoice>()
                .Where(x => x.UserProfile.Username == User.Identity.Name)
                .OrderByDescending(x => x.InvoiceNumber)
                .Select(x => new InvoicesInvoiceModel(x))
                .ToArray();

            return PartialView("_Load", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Add()
        {
            var user = _repository.Get<UserProfile>(x => x.Username == User.Identity.Name);
            if (user == null)
                throw new Exception("Username {0} not found".F(User.Identity.Name));

            var invoice = user.CreateNewInvoice();
            _uow.Commit();

            return PartialView("_Invoice", new InvoicesInvoiceModel(invoice));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public void Remove(int invoiceId)
        {
            var user = _repository.Get<UserProfile>(x => x.Username == User.Identity.Name);
            if (user == null)
                throw new Exception("Username {0} not found".F(User.Identity.Name));

            var invoice = user.Invoices.SingleOrDefault(x => x.Id == invoiceId);
            if (invoice == null)
                throw new Exception("Invoice {0} not found".F(invoiceId));

            user.Remove(invoice);
            _uow.Commit();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AddLineItem(int invoiceId)
        {
            var user = _repository.Get<UserProfile>(x => x.Username == User.Identity.Name);
            if (user == null)
                throw new Exception("Username {0} not found".F(User.Identity.Name));

            var invoice = user.Invoices.SingleOrDefault(x => x.Id == invoiceId);
            if (invoice == null)
                throw new Exception("Invoice {0} not found".F(invoiceId));

            invoice.CreateNewLineItem();
            _uow.Commit();

            var model = invoice.LineItems.Select(x => new InvoicesInvoiceModel.LineItem(x)).ToArray();

            return PartialView("_InvoiceLineItems", model);
        }

        public ActionResult CustomerEditForm(int invoiceId)
        {
            var user = _repository.Get<UserProfile>(x => x.Username == User.Identity.Name);
            if (user == null)
                throw new Exception("Username {0} not found".F(User.Identity.Name));

            var invoice = user.Invoices.SingleOrDefault(x => x.Id == invoiceId);
            if (invoice == null)
                throw new Exception("Invoice {0} not found".F(invoiceId));

            return PartialView(new InvoicesInvoiceModel.Customer(invoice.InvoiceCustomer ?? new InvoiceCustomer()));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CustomerEditForm(int invoiceId, InvoicesInvoiceModel.Customer model)
        {
            var user = _repository.Get<UserProfile>(x => x.Username == User.Identity.Name);
            if (user == null)
                throw new Exception("Username {0} not found".F(User.Identity.Name));

            var invoice = user.Invoices.SingleOrDefault(x => x.Id == invoiceId);
            if (invoice == null)
                throw new Exception("Invoice {0} not found".F(invoiceId));

            if (!ModelState.IsValid)
                return PartialView(model);

            invoice.UpdateCustomer(model.Name, model.Address.Line1, model.Address.Line2, model.Address.City, model.Address.State, model.Address.PostalCode);
            _uow.Commit();

            return PartialView("_InvoiceCustomer", model);
        }
    }
}
