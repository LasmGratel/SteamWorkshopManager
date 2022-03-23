using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;

namespace SteamWorkshopManager.Util;

public static class IOHelper
{
    public static async Task ClearDirectoryAsync(this StorageFolder dir)
    {
        await Task.WhenAll((await dir.GetItemsAsync()).Select(f => f.DeleteAsync().AsTask()));
    }


    public static IAsyncAction WriteStringAsync(this StorageFile storageFile, string str)
    {
        return storageFile.WriteBytesAsync(Encoding.UTF8.GetBytes(str));
    }

    public static IAsyncAction WriteBytesAsync(this StorageFile storageFile, byte[] bytes)
    {
        return FileIO.WriteBytesAsync(storageFile, bytes);
    }

    public static async Task<StorageFile> GetOrCreateFileAsync(this StorageFolder folder, string itemName)
    {
        return await folder.TryGetItemAsync(itemName) is StorageFile file ? file : await folder.CreateFileAsync(itemName, CreationCollisionOption.ReplaceExisting);
    }

    public static async Task<StorageFolder> GetOrCreateFolderAsync(this StorageFolder folder, string folderName)
    {
        return await folder.TryGetItemAsync(folderName) is StorageFolder f ? f : await folder.CreateFolderAsync(folderName, CreationCollisionOption.ReplaceExisting);
    }

    public static async Task<byte[]?> ReadBytesAsync(this StorageFile? file)
    {
        if (file == null)
        {
            return null;
        }

        using IRandomAccessStream stream = await file.OpenReadAsync();
        using var reader = new DataReader(stream.GetInputStreamAt(0));
        await reader.LoadAsync((uint)stream.Size);
        var bytes = new byte[stream.Size];
        reader.ReadBytes(bytes);
        return bytes;
    }

}