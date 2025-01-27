using MongoDB.Driver;

public class MongoDBService
{
    private readonly IMongoCollection<TodoItem> _todoItems;

    public MongoDBService(IConfiguration config)
    {
        // อ่านค่า Connection String และ Database Name จาก appsettings.json
        var client = new MongoClient(config["MongoDB:ConnectionString"]);
        var database = client.GetDatabase(config["MongoDB:DatabaseName"]);

        // เชื่อมต่อกับ Collection "tasks"
        _todoItems = database.GetCollection<TodoItem>("tasks");
    }

    public async Task<List<TodoItem>> GetAsync() =>
        await _todoItems.Find(_ => true).ToListAsync();

    public async Task<TodoItem?> GetAsync(string id) =>
        await _todoItems.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(TodoItem newItem) =>
        await _todoItems.InsertOneAsync(newItem);

    public async Task UpdateAsync(string id, TodoItem updatedItem) =>
        await _todoItems.ReplaceOneAsync(x => x.Id == id, updatedItem);

    public async Task DeleteAsync(string id) =>
        await _todoItems.DeleteOneAsync(x => x.Id == id);

    // ✅ เพิ่มฟังก์ชัน TestConnection
    public bool TestConnection()
    {
        try
        {
            _todoItems.Database.Client.ListDatabaseNames(); // ลองดึงรายชื่อ Database
            return true; // ถ้าสำเร็จ แสดงว่าเชื่อมต่อได้
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MongoDB Connection Error: {ex.Message}");
            return false;
        }
    }
}
