using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CRUD
{
    public class EntityRecord
    {
        public Entity Entity { get; private set; }

        private Dictionary<string, string> fields = new Dictionary<string, string>();
        private Dictionary<string, EntityRecord> recordFields = new Dictionary<string, EntityRecord>();

        private string id;
        internal string Id { get { return id; } }

        private string Path { get { return System.IO.Path.Combine(Entity.Path, id); } }
        private string FieldsPath { get { return System.IO.Path.Combine(Path, "fields.txt"); } }
        private string EntitiesPath { get { return System.IO.Path.Combine(Path, "ids.txt"); } }

        internal EntityRecord(string id, Entity entity)
        {
            this.id = id;
            this.Entity = entity;
        }

        internal void AddField(string name, string value)
        {
            fields.Add(name, value);
        }

        public string GetField(string name)
        {
            return fields[name];
        }

        public bool ContainsField(string name)
        {
            return fields.ContainsKey(name);
        }

        internal void AddRecordField(string name,EntityRecord entity)
        {
            recordFields.Add(name, entity);
        }

        public EntityRecord GetRecordField(string name)
        {
            return recordFields[name];
        }

        public bool ContainsRecordField(string name)
        {
            return recordFields.ContainsKey(name);
        }

        public void SetField(string key, object value)
        {
            var type = value.GetType();
            var method = type.GetMethod("Parse", new[] { typeof(string) });
            if (method != null || type.Equals(typeof(string)))
                fields[key] = value.ToString();
            else
                throw new MissingMethodException("Type should implement Parse(string) method");
        }

        internal void Save()
        {
            string path = Directory.CreateDirectory(Path).FullName;
            foreach (var entity in recordFields)
            {
                entity.Value.Save();
            }

            using (var writer = File.CreateText(FieldsPath))
            {
                foreach (var field in fields)
                    writer.WriteLine(field.Key + ":" + field.Value);
            }

            using (var writer = File.CreateText(EntitiesPath))
            {
                foreach (var entity in recordFields)
                    writer.WriteLine(entity.Key + ":" + entity.Value.id);
            }
        }

        internal static EntityRecord Load(string id, Entity entity)
        {
            EntityRecord record = new EntityRecord(id, entity);

            using (var reader = File.OpenText(record.FieldsPath))
            {
                string line = reader.ReadLine();
                while(line != null)
                {
                    string[] tokens = line.Split(':');
                    record.fields.Add(tokens[0], tokens[1]);
                    line = reader.ReadLine();
                }
            }

            Dictionary<string, string> ids = new Dictionary<string, string>();

            using (var reader = File.OpenText(record.EntitiesPath))
            {
                string line = reader.ReadLine();
                while(line != null)
                {
                    string[] tokens = line.Split(':');
                    ids.Add(tokens[0], tokens[1]);
                    line = reader.ReadLine();
                }
            }

            foreach(var recordField in ids)
            {
                record.recordFields.Add(recordField.Key, record.Entity.Storage.GetRecord(recordField.Value));
            }

            return record;
        }

        internal void Delete()
        {
            foreach (var file in Directory.EnumerateFiles(Path))
                File.Delete(file);

            Directory.Delete(Path);
        }
    }
}
