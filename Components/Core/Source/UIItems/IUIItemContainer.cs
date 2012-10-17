using System;
using White.Core.UIItems.Finders;

namespace White.Core.UIItems
{
    public interface IUIItemContainer : IUIItem
    {
        T Get<T>() where T : UIItem;
        T Get<T>(string primaryIdentification, TimeSpan? timeout = null) where T : UIItem;
        T Get<T>(SearchCriteria searchCriteria,TimeSpan? timeout = null) where T : UIItem;
    }
}