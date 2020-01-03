﻿using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using SAR.Libraries.Database.Repos;
using SAR.Modules.Script.Objects;

namespace SAR.Modules.Script.Repos
{
    public class ScriptElementRepo : AbstractRepo<ScriptElement>
    {
        public ScriptElementRepo(LiteDatabase db)
            : base(db, "ScriptElements")
        {
            Collection.EnsureIndex("ProjectId", false);
            Collection.EnsureIndex("SequenceNumber", false);
        }

        public IEnumerable<ScriptElement> GetByProject(Guid projectId)
        {
            Query q = Query.EQ("ProjectId", projectId);
            var items = Collection.Find(q);

            return items.OrderBy(i => i.SequenceNumber);
        }

        public void DeleteByProject(Guid projectId)
        {
            Query q = Query.EQ("ProjectId", projectId);
            Collection.Delete(q);
        }

        public IEnumerable<ScriptElement> GetByProject(Guid projectId, int? startPosition, int? endPosition)
        {
            int start = 0;
            if (startPosition != null)
            {
                start = startPosition.Value;
            }

            int end = int.MaxValue;
            if (endPosition != null)
            {
                end = endPosition.Value;
            }

            Query q = Query.And(
                Query.EQ("ProjectId", projectId),
                Query.GTE("SequenceNumber", start),
                Query.LT("SequenceNumber", end));

            var items = Collection.Find(q);
            return items.OrderBy(i => i.SequenceNumber);
        }
    }
}
