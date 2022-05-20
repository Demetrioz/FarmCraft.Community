namespace FarmCraft.Community.Services.Cache
{
    public interface ICacheService
    {
        /// <summary>
        /// Retrieve an item from the cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetItem(string key);

        /// <summary>
        /// Set an item in the cache using the default expiration
        /// </summary>
        /// <param name="key">The key to save the object under</param>
        /// <param name="value">The value to associate with the key</param>
        /// <returns></returns>
        public void SetItem(string key, object value);

        /// <summary>
        /// Set an item in the cache using a custom defined expiration
        /// </summary>
        /// <param name="key">The key to save the object under</param>
        /// <param name="value">The value to associate with the key</param>
        /// <param name="validFor">The duration the object is cached for</param>
        public void SetItem(string key, object value, TimeSpan validFor);

        /// <summary>
        /// Attempt to get an item from the cache and use the setter if it
        /// does not exist
        /// </summary>
        /// <param name="key">The key to save the value under</param>
        /// <param name="setter">A function that retrieves the value if it 
        /// doesn't already exist in the cache</param>
        /// <returns>An object pertaining to the supplied key</returns>
        public object GetAndSetItem(string key, Func<object> setter);

        /// <summary>
        /// Attempt to get an item from the cache and use the setter if it
        /// does not exist
        /// </summary>
        /// <param name="key">The key to save the value under</param>
        /// <param name="setter">A function that retrieves the value if it 
        /// doesn't already exist in the cache</param>
        /// <param name="validFor">The duration the object is cached for</param>
        /// <returns>An object pertaining to the supplied key</returns>
        public object GetAndSetItem(string key, Func<object> setter, TimeSpan validFor);
    }
}
