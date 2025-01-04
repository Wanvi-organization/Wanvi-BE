using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }
        public virtual ICollection<TourCategory> TourCategories { get; set; }
        public virtual ICollection<News> News { get; set; }
    }
}
