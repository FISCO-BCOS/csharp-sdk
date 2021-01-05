pragma solidity ^0.4.24;

contract DefaultTest{
    string name;
 event SetEvent(string paramsStr,uint256 operationTimeStamp);
    constructor() public{
       name = "Hello, World!";
    }

    function get() constant public returns(string){
        return name;
    }

    function set(string n) public{
    	name = n;
    	 emit  SetEvent(n,now);
    }
}