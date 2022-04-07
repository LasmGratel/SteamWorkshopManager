using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using SteamWorkshopManager.Client.Engine;
using SteamWorkshopManager.Core;

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


    public static UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    public static StreamWriter GetWriter(this Stream stream, Encoding? encoding = null, int bufferSize = -1, bool leaveOpen = false)
    {
        try
        {
            // https://docs.microsoft.com/zh-cn/dotnet/api/system.io.streamwriter.-ctor?view=net-5.0#System_IO_StreamWriter__ctor_System_IO_Stream_System_Text_Encoding_System_Int32_System_Boolean_
            // https://github.com/dotnet/corefx/blob/master/src/Common/src/CoreLib/System/IO/StreamWriter.cs#L94
            return new StreamWriter(stream, encoding, bufferSize, leaveOpen);
        }
        catch (Exception e) when (e is ArgumentNullException or ArgumentOutOfRangeException)
        {
            if (encoding == null)
            {
                encoding = Utf8NoBom;
            }
            if (bufferSize == -1)
            {
                bufferSize = 1024;
            }
            return new StreamWriter(stream, encoding, bufferSize, leaveOpen);
        }
    }

    public static async Task<Result<string>> GetResponseAsync(this SteamWorkshopClient client, string url)
    {
        try
        {
            var response = await client.SendRequest(url).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return Result<string>.OfFailure(new HttpRequestException("HTTP " + response.StatusCode));
            return Result<string>.OfSuccess(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }
        catch (HttpRequestException e)
        {
            return Result<string>.OfFailure(e);
        }
    }
}