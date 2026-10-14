using System;
using System.Collections.Generic;
using System.Text;

namespace CaliHub_Model
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }

        // בנאי לישות חדשה שעדיין אין לה Id ממסד הנתונים
        protected BaseEntity()
        {
        }

        // בנאי לישות קיימת שנשלפה ממסד הנתונים
        protected BaseEntity(int id)
        {
            Id = id;
        }
    }
}
