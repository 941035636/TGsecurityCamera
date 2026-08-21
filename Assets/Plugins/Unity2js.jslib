mergeInto(LibraryManager.library,{

	ChangerUrl:function(url){
	
	  // 在Unity中向js传递字符串时需要在js中使用Pointer_stringify(str)进行转换。
	  ChangeUrl(Pointer_stringify(url));
	}

	
});


