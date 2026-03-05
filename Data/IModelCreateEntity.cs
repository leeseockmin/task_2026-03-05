using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB.Data
{
    public interface IModelCreateEntity
    {
        public void CreateModel(ModelBuilder modelBuilder);
    }
}
