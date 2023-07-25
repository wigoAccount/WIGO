using System.Collections.Generic;
using UnityEngine;
using WIGO.Userinterface;

[CreateAssetMenu(menuName = "Content/Storage/Filters database")]
public class EventsFilterDatabase : ScriptableObject
{
    [SerializeField] EventCategory[] _categories;

    public IEnumerable<EventCategory> GetAllCategories() => _categories;
}
