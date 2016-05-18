using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace CRUD
{
    public class Entity
    {
        public string Name { get; private set; }
        private List<string> fields = new List<string>();
        private Dictionary<string, Entity> entityFields = new Dictionary<string, Entity>();
        private Storage storage;
        public Storage Storage { get { return storage; } }
        internal string Path { get { return System.IO.Path.Combine(storage.Path, Name); } }
        private string FieldsPath { get { return System.IO.Path.Combine(Path, "fields.txt"); } }
        private string EntityNamesPath { get { return System.IO.Path.Combine(Path, "entity_names.txt"); } }

        private Entity() { }

        internal Entity(string name, Storage storage)
        {
            this.Name = name;
            this.storage = storage;
            Directory.CreateDirectory(Path);
        }

        internal Entity(string name, Storage storage, List<string> fields, Dictionary<string, Entity> entityFields) : this(name, storage)
        {
            this.fields = fields;
            this.entityFields = entityFields;
            SaveFields();
            SaveEntities();
        }

        private void SaveFields()
        {
            using (var writer = File.CreateText(FieldsPath))
            {
                foreach (var field in fields)
                    writer.WriteLine(field);
            }
        }

        private void SaveEntities()
        {
            using (var writer = File.CreateText(EntityNamesPath))
            {
                foreach (var entity in entityFields)
                    writer.WriteLine(entity.Key + ":" + entity.Value.Name);
            }
        }

        public void DeclareField(string name)
        {
            fields.Add(name);
            SaveFields();
        }

        public void RemoveField(string name)
        {
            fields.Remove(name);
            SaveFields();
        }

        public void DeclareEntity(string name, Entity scheme)
        {
            entityFields.Add(name, scheme);
            SaveEntities();
        }

        public void RemoveEntity(string name)
        {
            entityFields.Remove(name);
            SaveEntities();
        }

        internal static Entity Load(Storage storage, string name)
        {
            Entity entity = new Entity();
            entity.Name = name;
            entity.storage = storage;

            using (var reader = File.OpenText(entity.FieldsPath))
            {
                string line = reader.ReadLine();
                while(line != null)
                {
                    entity.fields.Add(line);
                    line = reader.ReadLine();
                }
            }

            using (var reader = File.OpenText(entity.EntityNamesPath))
            {
                string line = reader.ReadLine();
                while(line != null)
                {
                    string[] tokens = line.Split(':');
                    entity.entityFields.Add(tokens[0], storage.GetEntity(tokens[1]));
                    line = reader.ReadLine();
                }
            }

            return entity;
        }

        public EntityRecord GetRecord(object obj)
        {
            EntityRecord ent = new EntityRecord(System.Guid.NewGuid().ToString(), this);
            Type objType = obj.GetType();

            foreach(var fieldName in fields)
            {
                var value = objType.GetField(fieldName).GetValue(obj);
                ent.AddField(fieldName, value.ToString());
            }

            foreach(var entityField in entityFields)
            {
                var entity = entityField.Value.GetRecord(objType.GetField(entityField.Key).GetValue(obj));
                ent.AddRecordField(entityField.Key, entity);
            }

            return ent;
        }

        public void SetFields(EntityRecord record, object obj)
        {
            Type objType = obj.GetType();

            foreach (var fieldName in fields)
            {
                var field = objType.GetField(fieldName);
                var type = field.FieldType;
                var parseMethod = type.GetMethod("Parse", new[] { typeof(string) });
                if (parseMethod != null)
                    field.SetValue(obj, parseMethod.Invoke(null, new[] { record.GetField(fieldName) }));
                else if (type.Equals(typeof(string)))
                    field.SetValue(obj, record.GetField(fieldName));
            }

            foreach(var entityField in entityFields)
            {
                entityField.Value.SetFields(record.GetRecordField(entityField.Key), objType.GetField(entityField.Key).GetValue(obj));
            }
        }
    }
}
