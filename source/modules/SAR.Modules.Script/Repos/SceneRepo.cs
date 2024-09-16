using System;
using System.Collections.Generic;
using LiteDB;
using SAR.Libraries.Database.Repos;
using SAR.Modules.Script.Objects;

namespace SAR.Modules.Script.Repos
{
    public class SceneRepo : AbstractRepo<Scene>
    {
        public SceneRepo(LiteDatabase db)
            : base(db, "Scenes")
        {
            Collection.EnsureIndex("ProjectId");
        }

        public IEnumerable<Scene> GetByProject(Guid projectId)
        {
            return Collection.Query()
                .Where(s => s.ProjectId == projectId)
                .OrderBy(s => s.ScriptSequenceNumber)
                .ToEnumerable();
        }

        public Scene GetByProjectNumber(Guid projectId, int sceneNumber)
        {
            var query = Query.And(
                Query.EQ("ProjectId", projectId),
                Query.EQ("Number", sceneNumber));

            return Collection.FindOne(query);
        }

        public void DeleteByProject(Guid projectId)
        {
            var q = Query.EQ("ProjectId", projectId);
            Collection.DeleteMany(q);
        }
    }
}
