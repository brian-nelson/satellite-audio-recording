import "./Script.css";
import React, {Component} from "react";
import SarService from "../Services/SarService";
import {Row, Col, Table} from "react-bootstrap";
import { Link } from 'react-router-dom'
import {toast} from "react-toastify";
import Scenes from "../Components/Scenes";
import DialogViewer from "../Components/DialogViewer";

export default class Script extends Component {
  constructor(props) {
    super(props);

    let projectId = this.props.match.params.projectId;

    this.state = {
      currentScene: null,
      scenes: [],
      dialog:[],
      projectId: projectId,
      childHeight: window.innerHeight - 100
    };

    this.handleSceneSelected = this.handleSceneSelected.bind(this);
    this.loadScenes = this.loadScenes.bind(this);
    this.loadDialog = this.loadDialog.bind(this);
    this.handleResize = this.handleResize.bind(this);

    window.addEventListener('resize', this.handleResize);

    this.handleResize();
  }

  componentWillUnmount() {
    window.removeEventListener('resize', this.handleResize);
  }

  handleResize() {
    let height = window.innerHeight - 100;

    this.setState({
      childHeight: height
    });
  }

  handleSceneSelected(scene) {
    this.setState({
      currentScene: scene
    }, ()=>{
      this.loadDialog();
    });
  }

  componentDidMount() {
    this.loadScenes();
  }

  loadScenes() {
    SarService.getScenes(this.state.projectId)
      .then(r => {
        this.setState({
          scenes: r
        });
      })
      .catch(e => {
        toast.error(e.message);
      });
  }

  loadDialog() {
    if (this.state.currentScene !== null) {
      SarService.getLinesByScene(this.state.projectId, this.state.currentScene.Id)
        .then(r => {
          this.setState({
            dialog: r
          });
        })
        .catch(e => {
          toast.error(e.message);
        });
    }
  }


  render() {
    return (
      <div className="Script">
        <Row>
          <Col>
            <h3>Script</h3>
          </Col>
          <Col>
          </Col>
        </Row>
        <Row>
          <Col md={3}>
            <Scenes
              onSceneSelected={this.handleSceneSelected}
              scenes={this.state.scenes}
              height={this.state.childHeight}
            />
          </Col>
          <Col md={9}>
            <DialogViewer
              dialog={this.state.dialog}
              height={this.state.childHeight}
            />
          </Col>
        </Row>
      </div>
    );
  }
}