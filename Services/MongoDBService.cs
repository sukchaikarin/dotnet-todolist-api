using MongoDB.Driver;

public class MongoDBService
{
    private readonly IMongoCollection<TodoItem> _todoItems;

   public MongoDBService(IConfiguration config)
{
    // อ่านค่าจาก Environment Variables > หรือ fallback ไปที่ appsettings.json
    var connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING") 
                            ?? config["MongoDB:ConnectionString"];
    var databaseName = Environment.GetEnvironmentVariable("MONGO_DATABASE_NAME") 
                            ?? config["MongoDB:DatabaseName"];

    if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseName))
    {
        throw new InvalidOperationException("MongoDB Connection String and/or Database Name is not configured.");
    }

    // สร้าง MongoClient และเชื่อมต่อกับฐานข้อมูล
    var client = new MongoClient(connectionString);
    var database = client.GetDatabase(databaseName);

    _todoItems = database.GetCollection<TodoItem>("tasks");
}

    // ✅ ดึงรายการทั้งหมด
    public async Task<List<TodoItem>> GetAsync() =>
        await _todoItems.Find(_ => true).ToListAsync();

    // ✅ ดึงข้อมูล Task ตาม ID
    public async Task<TodoItem?> GetAsync(string id) =>
        await _todoItems.Find(x => x.Id == id).FirstOrDefaultAsync();

    // ✅ สร้าง Task ใหม่
    public async Task CreateAsync(TodoItem newItem) =>
        await _todoItems.InsertOneAsync(newItem);

    // ✅ อัปเดต Task ตาม ID
    public async Task UpdateAsync(string id, TodoItem updatedItem) =>
        await _todoItems.ReplaceOneAsync(x => x.Id == id, updatedItem);

    // ✅ ลบ Task ตาม ID
    public async Task DeleteAsync(string id) =>
        await _todoItems.DeleteOneAsync(x => x.Id == id);

    // ✅ ฟังก์ชันสำหรับตรวจสอบสถานะการเชื่อมต่อ MongoDB
    public bool TestConnection()
    {
        try
        {
            // ทดสอบการเชื่อมต่อด้วยการดึงข้อมูล Database Names
            _todoItems.Database.Client.ListDatabaseNames();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MongoDB Connection Error: {ex.Message}");
            return false;
        }
    }
}
