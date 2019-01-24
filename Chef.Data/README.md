# Chef.Data

## 動態生成 UPDATE 語句

- 型別為 `Field<T>` 的屬性是要被更新的欄位，會出現在 SET 區塊，除此之外的屬性會被當做是 WHERE 條件。
- 預設以類別名稱為要更新的資料表名稱，如果要額外指定就在類別上標記 `[Table("...")]`。
- 預設以屬性名稱為欄位名稱，如果要額外指定就在屬性上標記 `[Column("...")]`。
- 被標記為 `[NotMapped]` 以及值為 `null` 的屬性會被略過

下面是一個範例：

    [Table("tblMember")]
    public class MemberUpdated
    {
        public int Id { get; set; }

        [Column("Tel")]
        public Field<string> Phone { get; set; }

        public Field<int> Age { get; set; }

        public Field<string> Name { get; set; }

        public Field<string> NickName { get; set; }

        [NotMapped]
        public string Address { get; set; }
    }

產生 UPDATE 語句就呼叫 `GenerateUpdateCommand()` 方法，下面是一個範例：

    var member = new MemberUpdated
                 {
                     Id = 1,
                     Phone = "02-22401139",
                     Age = 12,
                     Name = "Mary",
                     Address = "abcdefghijklmno..."
                 };

    var output = member.GenerateUpdateCommand(out var parameters);

    // ouput:
    //    UPDATE [tblMember]
    //    SET [Tel] = @Phone, [Age] = @Age, [Name] = @Name
    //    WHERE [Id] = @Id

    // parameters:
    //    [
    //      {
    //        "Key": "Id",
    //        "Value": 1
    //      },
    //      {
    //        "Key": "Phone",
    //        "Value": "02-22401139"
    //      },
    //      {
    //        "Key": "Age",
    //        "Value": 12
    //      },
    //      {
    //        "Key": "Name",
    //        "Value": "Mary"
    //      }
    //    ]

支援對象是集合型別，只要是有實作 `IEnumerable` 的對象，會輸出多個 UPDATE 語句。

    var members = new List<MemberUpdated>
                  {
                      new MemberUpdated
                      {
                          Id = 1,
                          Phone = "02-22401139",
                          Age = 12,
                          Name = "Mary",
                          Address = "abcdefghijklmno..."
                      },
                      new MemberUpdated
                      {
                          Id = 2,
                          Phone = "0912345678",
                          Age = 12,
                          Name = "Tom",
                          Address = "ddddaaaaddccadff..."
                      }
                  };

    var output = members.GenerateUpdateCommand(out var parameters);

    // output:
    //    UPDATE [tblMember]
    //    SET [Tel] = @Phone0, [Age] = @Age0, [Name] = @Name0
    //    WHERE [Id] = @Id0
    //    
    //    UPDATE [tblMember]
    //    SET [Tel] = @Phone1, [Age] = @Age1, [Name] = @Name1
    //    WHERE [Id] = @Id1

    // parameters:
    //    [
    //      {
    //        "Key": "Id0",
    //        "Value": 1
    //      },
    //      {
    //        "Key": "Phone0",
    //        "Value": "02-22401139"
    //      },
    //      {
    //        "Key": "Age0",
    //        "Value": 12
    //      },
    //      {
    //        "Key": "Name0",
    //        "Value": "Mary"
    //      },
    //      {
    //        "Key": "Id1",
    //        "Value": 2
    //      },
    //      {
    //        "Key": "Phone1",
    //        "Value": "0912345678"
    //      },
    //      {
    //        "Key": "Age1",
    //        "Value": 12
    //      },
    //      {
    //        "Key": "Name1",
    //        "Value": "Tom"
    //      }
    //    ]
