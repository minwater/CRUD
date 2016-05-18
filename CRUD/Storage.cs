using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CRUD
{
    public class Storage
    {
        private Dictionary<string, EntityRecord> records = new Dictionary<string, EntityRecord>();
        private Dictionary<string, Entity> nameEntities = new Dictionary<string, Entity>();
        private Dictionary<Entity, List<EntityRecord>> groupedRecords = new Dictionary<Entity, List<EntityRecord>>();

        private string path;
        public string Path { get { return path; } }

        public Storage(string path)
        {
            this.path = path;

            foreach (var directory in Directory.EnumerateDirectories(path))
            {
                string entityName = Directory.CreateDirectory(directory).Name;
                if (!nameEntities.ContainsKey(entityName))
                {
                    Entity entity = Entity.Load(this, entityName);
                    AddEntity(entity);
                    foreach (var recordDir in Directory.EnumerateDirectories(directory))
                    {
                        string id = Directory.CreateDirectory(recordDir).Name;
                        if (!records.ContainsKey(id))
                        {
                            EntityRecord record = EntityRecord.Load(id, entity);
                            AddRecord(record);
                        }
                    }
                }
            }
        }

        private void AddRecord(EntityRecord record)
        {
            records.Add(record.Id, record);
            groupedRecords[record.Entity].Add(record);
        }

        public void SaveRecord(EntityRecord record)
        {
            record.Save();
            AddRecord(record);
        }

        private void AddEntity(Entity entity)
        {
            nameEntities.Add(entity.Name, entity);
            groupedRecords.Add(entity, new List<EntityRecord>());
        }

        public Entity GetEntity(string name)
        {
            return nameEntities[name];
        }

        public List<Entity> GetEntities()
        {
            return groupedRecords.Keys.ToList();
        }

        internal EntityRecord GetRecord(string recordId)
        {
            return records[recordId];
        }

        public Entity CreateEntity(string name, List<string> fields, Dictionary<string, Entity> entityFields)
        {
            Entity entity = new Entity(name, this, fields, entityFields);
            AddEntity(entity);
            return entity;
        }

        public Entity CreateEntity(string name)
        {
            Entity entity = new Entity(name, this);
            AddEntity(entity);
            return entity;
        }

        public void DeleteRecord(EntityRecord record)
        {
            records.Remove(record.Id);
            groupedRecords[record.Entity].Remove(record);
            record.Delete();
        }

        public List<EntityRecord> GetAll(Entity entity)
        {
            return groupedRecords[entity];
        }

        public EntityRecord Find(Entity entity, string field, string value)
        {
            string[] tokens = field.Split('.');
            foreach (var record in groupedRecords[entity])
            {
                EntityRecord endRecord = record;
                for (int i = 0; i < tokens.Length - 1; i++)
                    endRecord = record.GetRecordField(tokens[i]);
                if (endRecord.GetField(tokens.Last()).Equals(value))
                    return record;
            }
            return null;
        }


        public List<EntityRecord> FindAll(Entity entity, string field, string value)
        {
            List<EntityRecord> records = new List<EntityRecord>();
            string[] tokens = field.Split('.');
            foreach (var record in groupedRecords[entity])
                {
                    EntityRecord endRecord = record;
                    for (int i = 0; i < tokens.Length - 1; i++)
                        endRecord = record.GetRecordField(tokens[i]);
                    if (endRecord.GetField(tokens.Last()).Equals(value))
                        records.Add(record);
                }
            return records;
        }

        public List<EntityRecord> Select(Entity entity, string field, Func<string, bool> checker)
        {
            List<EntityRecord> records = new List<EntityRecord>();
            string[] tokens = field.Split('.');
            foreach (var record in groupedRecords[entity])
                {
                    EntityRecord endRecord = record;
                    for (int i = 0; i < tokens.Length - 1; i++)
                        endRecord = record.GetRecordField(tokens[i]);
                    if (checker(endRecord.GetField(tokens.Last())))
                        records.Add(record);
                }
            return records;
        }
    }
}
