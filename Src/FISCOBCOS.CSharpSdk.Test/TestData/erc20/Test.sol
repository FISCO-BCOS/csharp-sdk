pragma solidity >=0.4.24 <0.6.11;

contract Test{
    string key;
    
    constructor(string memory name) public {
        
        key=name;
    }
    
    mapping(uint=>string) public list;
    
    function set(uint index, string memory remark) public{
        
        list[index]=remark;
    }
    
   function get(uint256 temp) public view returns(string memory tempKey){
        require(temp==1);
         return key;
    }
    

    function getBlcokNumber() public view returns(uint){
       return block.number;
    }
    
  
}