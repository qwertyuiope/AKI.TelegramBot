using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Collections.Generic;

namespace AKI.TelegramBot.Hosting.Abstract
{
    public abstract class ServiceCollectionWrapper : IServiceCollection
    {
        protected readonly IServiceCollection _serviceDescriptors;

        public ServiceCollectionWrapper(IServiceCollection serviceDescriptors)
        {
            _serviceDescriptors = serviceDescriptors;
        }
        ServiceDescriptor IList<ServiceDescriptor>.this[int index] { get => _serviceDescriptors[index]; set => _serviceDescriptors[index] = value; }
        public virtual int Count => _serviceDescriptors.Count;
        public virtual bool IsReadOnly => _serviceDescriptors.IsReadOnly;
        public virtual void Add(ServiceDescriptor item) => _serviceDescriptors.Add(item);
        public virtual void Clear() => _serviceDescriptors.Clear();
        public virtual bool Contains(ServiceDescriptor item) => _serviceDescriptors.Contains(item);
        public virtual void CopyTo(ServiceDescriptor[] array, int arrayIndex) => _serviceDescriptors.CopyTo(array, arrayIndex);
        public virtual IEnumerator<ServiceDescriptor> GetEnumerator() => _serviceDescriptors.GetEnumerator();
        public virtual int IndexOf(ServiceDescriptor item) => _serviceDescriptors.IndexOf(item);
        public virtual void Insert(int index, ServiceDescriptor item) => _serviceDescriptors.Add(item);
        public virtual bool Remove(ServiceDescriptor item) => _serviceDescriptors.Remove(item);
        public virtual void RemoveAt(int index) => _serviceDescriptors.RemoveAt(index);
        IEnumerator IEnumerable.GetEnumerator() => _serviceDescriptors.GetEnumerator();
    }
}
