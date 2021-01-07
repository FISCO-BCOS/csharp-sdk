pragma solidity >=0.4.24 <0.6.11;
import "./IERC20.sol";
import "./SafeMath.sol";

/**
 * @title Standard ERC20 token
 *
 * @dev Implementation of the basic standard token.
 * https://github.com/ethereum/EIPs/blob/master/EIPS/eip-20.md
 * Originally based on code by FirstBlood: https://github.com/Firstbloodio/token/blob/master/smart_contract/FirstBloodToken.sol
 */
contract ERC20 is IERC20 {
  using SafeMath for uint256;

  mapping (address => uint256) private _balances;
  mapping (address => mapping (address => uint256)) private _allowed;
  uint256 private _totalSupply;

  mapping(address=>uint256)public newestKeyIndex;
  mapping (address => mapping (uint256 => transObject)) private _accountTransLogs;

  struct transObject { 
   address sender;
   address receiver;
   bytes32 typeName;
   uint256 amount;
   uint256 balance;
   uint256 blockNumber;
  }
  /**
  * @dev Total number of tokens in existence
  */
  function totalSupply() public view returns (uint256) {
    return _totalSupply;
  }

  /**
  * @dev Gets the balance of the specified address.
  * @param owner The address to query the balance of.
  * @return An uint256 representing the amount owned by the passed address.
  */
  function balanceOf(address owner) public view returns (uint256) {
    return _balances[owner];

  }

  /**
   * @dev Function to check the amount of tokens that an owner allowed to a spender.
   * @param owner address The address which owns the funds.
   * @param spender address The address which will spend the funds.
   * @return A uint256 specifying the amount of tokens still available for the spender.
   */
  function allowance(
    address owner,
    address spender
   )
    public
    view
    returns (uint256)
  {
    return _allowed[owner][spender];
  }

  //这一个随着数据量大，会爆发，废弃
  function getTransLogs(address account)public view returns(address[] memory,address[] memory,bytes32[] memory,uint256[] memory,uint256[] memory){
       address[] memory senderList = new address[](uint256(newestKeyIndex[account]));
       address[] memory receiverList = new address[](uint256(newestKeyIndex[account]));
       bytes32[] memory typeNameList = new bytes32[](uint256(newestKeyIndex[account]));
       uint256[] memory amountList = new uint256[](uint256(newestKeyIndex[account]));
       uint256[] memory  balanceList = new uint256[](uint256(newestKeyIndex[account]));
        for(uint256  i=0;i<newestKeyIndex[account];++i){
           senderList[i]=_accountTransLogs[account][i+1].sender;
           receiverList[i]=_accountTransLogs[account][i+1].receiver;
           typeNameList[i]=_accountTransLogs[account][i+1].typeName;
           amountList[i]=_accountTransLogs[account][i+1].amount;
           balanceList[i]=_accountTransLogs[account][i+1].balance;
      }
       return (senderList,receiverList,typeNameList,amountList,balanceList);
  
  }
    //获取最近一次交易索引
     function getLastestTransIndex(address account)public view returns(uint256){
          return newestKeyIndex[account];
     }
     //获取指定索引的交易
    function getIndexTransLogs(address account,uint256 indexKey)public view returns(address,address,bytes32,uint256,uint256){
     address tempSender= _accountTransLogs[account][indexKey].sender;
     address tempReceiver= _accountTransLogs[account][indexKey].receiver;
     bytes32 tempTypeName=_accountTransLogs[account][indexKey].typeName;
     uint256 tempAmount=_accountTransLogs[account][indexKey].amount;
     uint256 tempBalance=_accountTransLogs[account][indexKey].balance;
     return(tempSender,tempReceiver,tempTypeName,tempAmount,tempBalance);
    }
  
  //记录交易
   function setTransRecord(address owner,address from,address to,uint256 amount,bytes32 typeName) internal{
         _accountTransLogs[owner][newestKeyIndex[owner]+1].sender=from;
         _accountTransLogs[owner][newestKeyIndex[owner]+1].receiver=to;
         _accountTransLogs[owner][newestKeyIndex[owner]+1].typeName=typeName;
         _accountTransLogs[owner][newestKeyIndex[owner]+1].amount=amount;
         if((typeName=="初始发行"||typeName=="增加")&&to==owner){
          _accountTransLogs[owner][newestKeyIndex[owner]+1].balance= _accountTransLogs[owner][newestKeyIndex[owner]].balance+amount;
         }else if((typeName=="扣除"||typeName=="销毁")&& from==owner){
           _accountTransLogs[owner][newestKeyIndex[owner]+1].balance= _accountTransLogs[owner][newestKeyIndex[owner]].balance-amount;
         }
         newestKeyIndex[owner]=newestKeyIndex[owner]+1;
         } 

  /**
  * @dev Transfer token for a specified address
  * @param to The address to transfer to.
  * @param value The amount to be transferred.
  */
  function transfer(address to, uint256 value) public returns (bool) {
    require(value <= _balances[msg.sender]);
    require(to != address(0));

        _balances[msg.sender] = _balances[msg.sender].sub(value);
        _balances[to] = _balances[to].add(value);
        setTransRecord(msg.sender,msg.sender,to,value,"扣除");
        setTransRecord(to,msg.sender,to,value,"增加");
    emit Transfer(msg.sender, to, value);
    return true;
  }

  /**
   * @dev Approve the passed address to spend the specified amount of tokens on behalf of msg.sender.
   * Beware that changing an allowance with this method brings the risk that someone may use both the old
   * and the new allowance by unfortunate transaction ordering. One possible solution to mitigate this
   * race condition is to first reduce the spender's allowance to 0 and set the desired value afterwards:
   * https://github.com/ethereum/EIPs/issues/20#issuecomment-263524729
   * @param spender The address which will spend the funds.
   * @param value The amount of tokens to be spent.
   */
  function approve(address spender, uint256 value) public returns (bool) {
    require(spender != address(0));

    _allowed[msg.sender][spender] = value;
    emit Approval(msg.sender, spender, value);
    return true;
  }

  /**
   * @dev Transfer tokens from one address to another
   * @param from address The address which you want to send tokens from
   * @param to address The address which you want to transfer to
   * @param value uint256 the amount of tokens to be transferred
   */
  function transferFrom(
    address from,
    address to,
    uint256 value
  )
    public
    returns (bool)
  {
    require(value <= _balances[from]);
    require(value <= _allowed[from][msg.sender]);
    require(to != address(0));

    _balances[from] = _balances[from].sub(value);
    _balances[to] = _balances[to].add(value);
    _allowed[from][msg.sender] = _allowed[from][msg.sender].sub(value);

           setTransRecord(from,from,to,value,"扣除");
           setTransRecord(to,from,to,value,"增加");
    emit Transfer(from, to, value);
    return true;
  }

  /**
   * @dev Increase the amount of tokens that an owner allowed to a spender.
   * approve should be called when allowed_[_spender] == 0. To increment
   * allowed value is better to use this function to avoid 2 calls (and wait until
   * the first transaction is mined)
   * From MonolithDAO Token.sol
   * @param spender The address which will spend the funds.
   * @param addedValue The amount of tokens to increase the allowance by.
   */
  function increaseAllowance(
    address spender,
    uint256 addedValue
  )
    public
    returns (bool)
  {
    require(spender != address(0));

    _allowed[msg.sender][spender] = (
      _allowed[msg.sender][spender].add(addedValue));
    emit Approval(msg.sender, spender, _allowed[msg.sender][spender]);
    return true;
  }

  /**
   * @dev Decrease the amount of tokens that an owner allowed to a spender.
   * approve should be called when allowed_[_spender] == 0. To decrement
   * allowed value is better to use this function to avoid 2 calls (and wait until
   * the first transaction is mined)
   * From MonolithDAO Token.sol
   * @param spender The address which will spend the funds.
   * @param subtractedValue The amount of tokens to decrease the allowance by.
   */
  function decreaseAllowance(
    address spender,
    uint256 subtractedValue
  )
    public
    returns (bool)
  {
    require(spender != address(0));

    _allowed[msg.sender][spender] = (
      _allowed[msg.sender][spender].sub(subtractedValue));
    emit Approval(msg.sender, spender, _allowed[msg.sender][spender]);
    return true;
  }

  /**
   * @dev Internal function that mints an amount of the token and assigns it to
   * an account. This encapsulates the modification of balances such that the
   * proper events are emitted.
   * @param account The account that will receive the created tokens.
   * @param amount The amount that will be created.
   */
  function _mint(address account, uint256 amount) internal {
    require(account!= address(0));
    _totalSupply = _totalSupply.add(amount);
    _balances[account] = _balances[account].add(amount);
    setTransRecord(account,address(0),account,amount,"初始发行");
    emit Transfer(address(0), account, amount);
  }
        
  /**
   * @dev Internal function that burns an amount of the token of a given
   * account.
   * @param account The account whose tokens will be burnt.
   * @param amount The amount that will be burnt.
   */
  function _burn(address account, uint256 amount) internal {
    require(account != address(0));
    require(amount <= _balances[account]);

    _totalSupply = _totalSupply.sub(amount);
    _balances[account] = _balances[account].sub(amount);
     setTransRecord(account,account,address(0),amount,"销毁");
    emit Transfer(account, address(0), amount);
  }

  /**
   * @dev Internal function that burns an amount of the token of a given
   * account, deducting from the sender's allowance for said account. Uses the
   * internal burn function.
   * @param account The account whose tokens will be burnt.
   * @param amount The amount that will be burnt.
   */
  function _burnFrom(address account, uint256 amount) internal {
    require(amount <= _allowed[account][msg.sender]);

    // Should https://github.com/OpenZeppelin/zeppelin-solidity/issues/707 be accepted,
    // this function needs to emit an event with the updated approval.
    _allowed[account][msg.sender] = _allowed[account][msg.sender].sub(
      amount);
    _burn(account, amount);
  }
}