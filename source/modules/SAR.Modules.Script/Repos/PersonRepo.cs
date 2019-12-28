﻿using LiteDB;
using SAR.Libraries.Database.Repos;
using SAR.Modules.Script.Objects;

namespace SAR.Modules.Script.Repos
{
    public class PersonRepo : AbstractRepo<Person>
    {
        public PersonRepo(LiteDatabase db)
            : base(db, "Persons")
        {

        }
    }
}
