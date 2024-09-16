using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using SAR.Libraries.Database.Objects;

namespace SAR.Libraries.Database.Repos
{
    public abstract class AbstractRepo<T>
        where T:AbstractDbObject
    {
        protected readonly ILiteCollection<T> Collection;
        private readonly string _collectionName;

        protected AbstractRepo(
            LiteDatabase db, 
            string collectionName)
        {
            _collectionName = collectionName;
            Collection = db.GetCollection<T>(_collectionName);
        }
        
        public T GetById(Guid id)
        {
            var obj = Collection.FindById(id);
            return (T) obj;
        }

        public IEnumerable<T> GetAll()
        {
            var result = Collection
                .FindAll()
                .ToList();

            return result;
        }

        public void Save(T record)
        {
            if (record.Id.Equals(Guid.Empty))
            {
                throw new Exception("Empty guid attempted to be saved");
            }

            var existing = GetById(record.Id);

            if (existing == null)
            {
                Collection.Insert(record);
            }
            else
            {
                Collection.Update(record);
            }
        }

        public void Delete(Guid id)
        {
            Collection.Delete(id);
        }
    }
}
