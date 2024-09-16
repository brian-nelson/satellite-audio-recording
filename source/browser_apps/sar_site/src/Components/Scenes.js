import React from "react";
import {Row, Col, Button} from "react-bootstrap";
import SarService from "../Services/SarService";

export default class Scenes extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      scenes:[],
      selectedScene: null,
    };

    this.handleClick = this.handleClick.bind(this);
  }

  componentDidMount() {

  }

  componentWillUnmount() {

  }

  handleClick(obj) {
    let scene = null;
    let id = obj.currentTarget.id;
    let scenes = this.state.scenes;

    for (let i = 0; i < scenes.length; i++){
      let current = scenes[i];

      if (id === current.Id) {
        scene = current;
        break;
      }
    }

    this.setState({
      selectedScene: scene
    }, ()=> {
      this.props.onSceneSelected(scene);
    });
  }

  componentDidUpdate(prevProps, prevState, snapshot) {
    if (prevProps.scenes=== null && this.props.scenes !== null) {
      this.setState({
        scenes: this.props.scenes
      });
    } else if (prevProps.scenes !== null && this.props.scenes === null) {
      this.setState({
        scenes:[]
      });
    } else if (prevProps.scenes.length !== this.props.scenes.length) {
      this.setState({
        scenes: this.props.scenes
      });
    }
  }

  renderScenes(scenes) {
    if (scenes !== null
      && scenes.length > 0) {
      let rows =  scenes.map((item, i) => {
        const line = SarService.getSceneLine(item);

        let style = {
          height: "50px",
          width: "100%",
        };

        if (this.state.selectedScene !== null
            && this.state.selectedScene.Id === item.Id)
        {
          return (
            <Row
              key={i}
              style={{paddingBottom:"5px"}}>
              <Col>
                <Button
                  id={item.Id}
                  size="sm"
                  variant="primary"
                  style={style}
                  onClick={this.handleClick}
                >
                  {line}
                </Button>
              </Col>
            </Row>
          );
        } else {
          return (
            <Row
              key={i}
              style={{paddingBottom:"5px"}}>
              <Col>
                <Button
                  id={item.Id}
                  size="sm"
                  variant="secondary"
                  style={style}
                  onClick={this.handleClick}
                >
                  {line}
                </Button>
              </Col>
            </Row>
          );
        }
      });

      return (
        <Row style={{overflow:"scroll",height:this.props.height - 60}}>
          <Col>
            {rows}
          </Col>
        </Row>);
    }

    return (<Row></Row>);
  }

  render() {
    return (
      <Row style={{height: this.props.height}}>
        <Col>
          <Row>
            <Col>
              <h4>Scenes</h4>
            </Col>
          </Row>
          { this.renderScenes(this.state.scenes) }
        </Col>
      </Row>
    );
  }
}