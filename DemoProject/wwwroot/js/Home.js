
function Search(pg, finder, sort) {
    
    var pagesize = $('#selectentities').find(":selected").val();
    var obj = GetFilter();

    if (pg == undefined) {
        pg = 1;
    }
    
    console.log(pagesize);
    $.ajax({
        url: "/Home/Search",
        type: "post",
        data: {
            SearchFname: obj.SearchFname,
            SearchLname: obj.SearchLname,
            SearchEmail: obj.SearchEmail,
            Pg: pg,
            Finder: finder,
            Sort: sort,
            PageSize: pagesize
        },
        success: function (data) {
            $('#dataUsers').html(data);
        }
    })
    $.ajax({
        url: "/Home/Pagination",
        type: "post",
        data: {
            SearchFname: obj.SearchFname,
            SearchLname: obj.SearchLname,
            SearchEmail: obj.SearchEmail,
            Pg: pg,
            Finder: finder,
            Sort: sort,
            PageSize: pagesize
        },
        success: function (data) {
            $('#userpagination').html(data);
        }
    })
}

function downloadData(format, pageSize, currentPage) {
    
    var selectedFormat = format;
    var obj = GetFilter();

    $.ajax({
        url: '/Home/DownloadData',
        type: 'POST',
        data: {
            format: selectedFormat,
            SearchFname: obj.SearchFname,
            SearchLname: obj.SearchLname,
            SearchEmail: obj.SearchEmail,
            PageSize: pageSize,
            Pg: currentPage
        },
        success: function () {
            console.log('Download success');
        },
        error: function () {
            console.log('Download failed');
        }
    });
}

function GetFilter() {
    var SearchFname = $("input[name='SearchFname']").val();
    var SearchLname = $("input[name='SearchLname']").val();
    var SearchEmail = $("input[name='SearchEmail']").val();

    var searches = [];

    if (SearchFname.length > 0) {
        searches.SearchFname = SearchFname;
    }

    if (SearchLname.length > 0) {
        searches.SearchLname = SearchLname;
    }

    if (SearchEmail.length > 0) {
        searches.SearchEmail = SearchEmail;
    }

    return searches;
}

function GetCity() {

    var countryId = $('#ucountries').find(":selected").val();   
    $.ajax({

        url: "/Home/GetCity",
        method: "POST",

        data: {
            "countryId": countryId
        },

        success: function (data) {

            data = JSON.parse(data);
            $("#selectCityList").empty();

            data.forEach((name) => {

                document.getElementById("selectCityList").innerHTML += `
                                                           <option value="${name.CityId}" name="CityId">
                                                                   ${name.Name}
                                                           </option>`;
            })


        },
        error: function (request, error) {
            console.log(error);
        }

    })
}

function addUser() {

    var fname = $('#ufname').val().trim();
    var lname = $('#ulname').val().trim();
    var email = $('#uemail').val().trim();
    var password = $('#upassword').val().trim();
    var country = $('#ucountries').find(":selected").val();
    var city = $('#selectCityList').find(":selected").val();

    if (!(fname) || !(lname) || !(email) || !(password) || !(country) || !(city)) {
        toastr.error("Please Enter All The Data!!")
    } else {
        $.ajax({
            url: '/Home/AddUserByMe',
            type: 'POST',
            datatype: 'html',
            data: {
                Fname: fname,
                Lname: lname,
                Email: email,
                Password: password,
                CountryId: country,
                CityId: city
            },
            success: function () {
                location.reload()
            }

        });
    }


}

function clearModal() {
    $("#userAdd").trigger("reset");
}

function GetUserData(userId) {

    $.ajax({
        url: '/Home/GetDataOfUser',
        type: 'POST',
        datatype: 'json',
        data: {
            userId: userId
        },
        success: function (data) {

            var json = JSON.parse(data);

            console.log(json);

            if (json[0].FirstName != null) {
                document.getElementById('fname').value = json[0].FirstName;
            } else {
                document.getElementById('fname').value = "";
            }

            if (json[0].LastName != null) {
                document.getElementById('lname').value = json[0].LastName;
            } else {
                document.getElementById('lname').value = "";
            }

            if (json[0].Email != null) {
                document.getElementById('email').value = json[0].Email;
            } else {
                document.getElementById('email').value = "";
            }
            
            $("#selectCityLists").empty();
            document.getElementById('selectCityLists').innerHTML += `<option selected value="${json[0].CityId}">${json[0].CityName} </option>
                `;  
            document.getElementById('countries').value = json[0].CountryId;
            document.getElementById('userid').value = json[0].UserId;
        }

    })
}

function AfterEditAddUser() {

    var fname = $('#fname').val().trim();
    var lname = $('#lname').val().trim();
    var email = $('#email').val().trim();
    var countryId = $('#countries').find(":selected").val();
    var cityId = $('#selectCityLists').find(":selected").val();
    var userId = $('#userid').val().trim();

     if (!(fname) || !(lname) || !(email) || !(countryId) || !(cityId)) {
        toastr.error("Please Enter All The Data!!");
    }
    else {
        $.ajax({
            url: '/Home/EditUserByMe',
            type: 'POST',
            datatype: 'html',
            data: {
                Fname: fname,
                Lname: lname,
                Email: email,
                CountryId: countryId,
                CityId: cityId,
                UserId: userId,
            },
            success: function () {
                location.reload()
            }

        });
    }

}

function removeByAdmin(Id) {

   
        if (confirm("Do you want to remove This User ?")) {
            console.log(Id);

            $.ajax({

                url: '/Home/RemoveByAdmin',
                type: 'POST',
                datatype: 'html',
                data: {
                    Id: Id,
                },
                success: function () {
                    location.reload();
                }

            });
        }
        else {
            toastr.error("You have clicked on cancel!!")
        }

    
}

function GetCityForUser() {

    var countryId = $('#countries').find(":selected").val();

    $.ajax({

        url: "/Home/GetCity",
        method: "POST",

        data: {
            "countryId": countryId
        },

        success: function (data) {

            data = JSON.parse(data);
            $("#selectCityLists").empty();

            data.forEach((name) => {

                document.getElementById("selectCityLists").innerHTML += `
                                                                           <option value="${name.CityId}">
                                                                                   ${name.Name}
                                                                           </option>`;
            })


        },
        error: function (request, error) {
            console.log(error);
        }


    })
}

function AddProducts() {
    var ProductId = $('#products').find(":selected").val();
    var country = $('#ucountries').find(":selected").val();
    var city = $('#selectCityList').find(":selected").val();

    if (!(country) || !(city)) {
        toastr.error("Please Enter All The Data!!")
    } else {
        $.ajax({
            url: '/Home/OrderProduct',
            type: 'GET',
            datatype: 'html',
            data: {
                ProductId: ProductId,
                CountryId: country,
                CityId: city
            },
            success: function () {
                location.reload()
            }

        });
    }

}