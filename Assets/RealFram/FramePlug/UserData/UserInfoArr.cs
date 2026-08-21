using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
{ 
"records":
       [
         { 
           "id":6,
           "idNum":"234230",
           "username":"kll",
           "faceBase64":"ss",
           "salaryNum":"234",
           "phoneNum":"123123",
           "unitName":"stgergsdg",
           "address":"ser",
           "remarks":"wu",
           "auth":
                 [
                   { 
                      "id":null,
                       "type":1,
                       "authDate":"2023-01-01 2099-01-01",
                       "authTime":"00:00:00 23:59:59",
                        "authWeek":"all", 
                        "idNum":"234230"
                   }, 
                   { 
                      "id":null,
                      "type":3,
                      "authDate":"2023-01-01 2099-01-01",
                      "authTime":"00:00:00 23:59:59",
                      "authWeek":"1,2,3", 
                      "idNum":"234230"
                   }
                 ]
          },
          { 
          "id":7,
         "idNum":"234232",
         "username":"kll",
         "faceBase64":"a",
         "salaryNum":"234",
         "phoneNum":"123123",
         "unitName":"stgergsdg",
         "address":"ser",
         "remarks":"wu",
              "auth":
                [
                   { 
                      "id":null,
                      "type":1,
                      "authDate":"2023-01-01 2099-01-01",
                      "authTime":"00:00:00 23:59:59",
                       "authWeek":"all",
                       "idNum":"234232"
                    },
                    { 
                       "id":null,
                       "type":3,
                       "authDate":"2023-01-01 2099-01-01",
                       "authTime":"00:00:00 23:59:59",
                        "authWeek":"1,2,3", 
                       "idNum":"234232"
                    }
                ]
            }
      ],
        "total":6,
        "size":2,
        "current":1,
        "orders":[],
        "optimizeCountSql":true,
        "searchCount":true,
        "maxLimit":null,
        "countId":null,
        "pages":3
    }


 
 */

[Serializable]
public class UserInfoArr
{
    public List<UserInfo> records;
    public int total;
    public int size;
    public int current;
    public List<object> orders;
    public bool optimizeCountSql;
    public bool searchCount;
    public object maxLimit;
    public string countId;
    public int pages;
    public UserInfoArr(List<UserInfo> arr)
    {
        this.records = arr;
    }
}
