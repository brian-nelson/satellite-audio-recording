import React from "react";
import {Row, Col} from "react-bootstrap";

export default class DialogViewer extends React.Component {

  constructor(props) {
    super(props);

    this.state = {
      dialog:[]
    };
  }

  componentDidMount() {

  }

  componentDidUpdate(prevProps, prevState, snapshot) {
    if (prevProps.dialog.length !== this.props.dialog.length) {
      this.setState({
        dialog: this.props.dialog
      })

      return;
    }

    if (this.props.dialog.length > 0
        && prevProps.dialog.length > 0
        && this.props.dialog[0].Line !== prevProps.dialog[0].Line) {
      this.setState({
        dialog: this.props.dialog
      })
    }
  }

  renderDialog(list) {
    if (list !== null
      && list.length > 0) {
      let rows =  list.map((item, i) => {
        if (item.LineType === 'Scene') {
          return(
            <Row key={i}>
              <Col style={{
                fontWeight:'bold',
                paddingTop: '5px',
                paddingBottom: '5px',
                backgroundColor:'white'}} >
                {item.Line}
              </Col>
            </Row>);
        } else if (item.LineType === 'Character') {
          return(
            <Row key={i}>
              <Col style={{
                textAlign:'center',
                paddingTop: '5px',
                paddingBottom: '0px',
                backgroundColor:'white'}}>
                {item.Line}
              </Col>
            </Row>);
        } else if (item.LineType === 'Dialogue') {
            return (
              <Row key={i}>
                <Col style={{
                  backgroundColor:'white',
                  paddingTop:'0px',
                  paddingBottom:'5px',
                  paddingLeft:'100px',
                  paddingRight:'100px'
                }}>
                  {item.Line}
                </Col>
              </Row>);
        } else if (item.LineType === 'Action') {
          return (
            <Row key={i}>
              <Col style={{
                paddingTop: '5px',
                paddingBottom: '5px',
                backgroundColor:'white'}}>
                {item.Line}
              </Col>
            </Row>);
        } else if (item.LineType === 'Parenthetical') {
          return (
            <Row key={i}>
              <Col style={{
                paddingLeft: '100px',
                paddingRight: '100px',
                paddingTop: '0px',
                paddingBottom: '5px',
                textAlign:"center",
                backgroundColor:'white'}}>
                ({item.Line})
              </Col>
            </Row>);
        } else if (item.LineType === 'Transition') {
          return (
            <Row key={i}>
              <Col style={{
                paddingTop: '5px',
                paddingBottom: '5px',
                backgroundColor:'white'}}>
                {item.Line}
              </Col>
            </Row>);
        } else if (item.LineType === 'PageBreak') {
          return (
            <Row key={i}>
              <Col style={{
                paddingTop: '5px',
                paddingBottom: '5px',
                backgroundColor:'white'}}>
                <hr/>
              </Col>
            </Row>);
        }
        else {
          console.info(item);
          return (<Row key={i}><Col>Unknown: {item.LineType}</Col></Row>)
        }
      });

      return (
        <Row style={{
          overflow:"scroll",
          height:this.props.height - 60,
          paddingRight:'20px',
          paddingLeft:'20px'
        }}>
          <Col>
            {rows}
          </Col>
        </Row>);
    }

    return(
      <Row/>);
  }

  render() {
    return (
      <Row style={{
        marginLeft:0,
        marginRight:0,
        height:this.props.height}}>
        <Col>
          <Row>
            <Col>
              <h4>Dialog</h4>
            </Col>
          </Row>
          { this.renderDialog(this.state.dialog) }
        </Col>
      </Row>
    );
  }
}