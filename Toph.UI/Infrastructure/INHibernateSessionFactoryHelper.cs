using System;
using FluentNHibernate;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using NHibernate;
using Toph.Common;
using Toph.Domain.Entities;

namespace Toph.UI.Infrastructure
{
    public interface INHibernateSessionFactoryHelper
    {
        ISessionFactory CurrentSessionFactory { get; }
    }

    public class NHibernateSessionFactoryHelper : INHibernateSessionFactoryHelper
    {
        private ISessionFactory _currentSessionFactory;

        public ISessionFactory CurrentSessionFactory
        {
            get { return _currentSessionFactory ?? (_currentSessionFactory = CreateSessionFactory()); }
        }

        private static ISessionFactory CreateSessionFactory()
        {
            var entityBaseType = typeof(EntityBase);

            var autoPersistenceModel = AutoMap
                .AssemblyOf<EntityBase>()
                .Where(entityBaseType.IsAssignableFrom)
                .Conventions.Add<MyIdConvention>()
                .Conventions.Add<MyForeignKeyConvention>()
                .Conventions.Add<MyCollectionConvention>()
                .Override<Invoice>(map => map.Component(x => x.InvoiceCustomer, m =>
                {
                    m.Map(x => x.Name);
                    m.Map(x => x.Line1);
                    m.Map(x => x.Line2);
                    m.Map(x => x.City);
                    m.Map(x => x.State);
                    m.Map(x => x.PostalCode);
                }));

            return Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2008.ConnectionString(x => x.FromConnectionStringWithKey("toph_conn")))
                .Mappings(map => map.AutoMappings.Add(autoPersistenceModel))
                .BuildSessionFactory();
        }

        public class MyIdConvention : IIdConvention
        {
            public void Apply(IIdentityInstance instance)
            {
                instance.CustomType<int>();
                instance.GeneratedBy.Identity();
            }
        }

        public class MyForeignKeyConvention : ForeignKeyConvention
        {
            protected override string GetKeyName(Member property, Type type)
            {
                return "{0}Id".F(property != null ? property.Name : type.Name);
            }
        }

        public class MyCollectionConvention : ICollectionConvention
        {
            public void Apply(ICollectionInstance instance)
            {
                instance.Access.CamelCaseField(CamelCasePrefix.Underscore);
                instance.Cascade.AllDeleteOrphan();
                instance.Inverse();
            }
        }
    }
}
