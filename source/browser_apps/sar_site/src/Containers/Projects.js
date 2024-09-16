import "./Projects.css";
import React, {Component} from "react";
import SarService from "../Services/SarService";
import {Row, Col, Table} from "react-bootstrap";
import { Link } from 'react-router-dom'
import { FaPlusCircle } from "react-icons/fa";
import {toast} from "react-toastify";
import { Redirect } from "react-router-dom";

export default class Projects extends Component {
  constructor(props) {
    super(props);

    this.state = {
      projects:[],
      newPressed: false
    };

    this.loadData = this.loadData.bind(this);
    this.handleNewProject = this.handleNewProject.bind(this);
  }

  componentDidMount() {
    this.loadData();
  }

  handleNewProject() {
    this.setState({
      newPressed: true
    });
  }

  loadData() {
    SarService.getProjects()
      .then(r => {
        this.setState({
          projects: r
        });
      })
      .catch(e => {
        toast.error(e.message);
      });
  }

  renderTitleRow(projectId, title) {
    if (SarService.isProjectOwner(projectId)) {
      let projectUrl = `/projects/${projectId}`;

      return (
        <Row>
          <Col>
            <h4><Link to={projectUrl}>{title}</Link></h4>
          </Col>
        </Row>);
    } else {
      return (
        <Row>
          <Col>
            <h4>{title}</h4>
          </Col>
        </Row>);
    }
  }

  renderRedirect() {
    if (this.state.newPressed) {
      return <Redirect to="projects/new"/>;
    }
    return "";
  }

  renderTableBody(list) {
    if (list != null
        && list.length > 0) {
      let rows =  list.map((item, i) => {

        if (item.Id != null) {
          let projectUrl = `/projects/${item.Id}`;

          return (
            <tr key={item.Id}>
              <td>
                { this.renderTitleRow(item.Id, item.Title)}
                <Row>
                  <Col sm={3}><Link to={`${projectUrl}/script`}>Script</Link></Col>
                  <Col sm={3}><Link to={`${projectUrl}/characters`}>Characters</Link></Col>
                  <Col sm={3}><Link to={`${projectUrl}/participants`}>Participants</Link></Col>
                </Row>
              </td>
              <td>{item.Description}</td>
            </tr>
          );
        } else {
          return "";
        }
      });

      return(
        <tbody>
        {rows}
        </tbody>);
    }

    return(
      <tbody></tbody>);
  }


  render() {
    return (
      <div className="Projects">
        {this.renderRedirect()}
        <Row>
          <Col>
            <h3>Projects</h3>
          </Col>
          <Col>
            <div className="float-md-right">
              <span id="newProject" onClick={() => this.handleNewProject()}>
                <FaPlusCircle/>
              </span>
            </div>
          </Col>
        </Row>
        <Row>
          <Col>
            <Table striped bordered hover>
              <thead>
              <tr>
                <th>Title</th>
                <th>Description</th>
              </tr>
              </thead>
              { this.renderTableBody(this.state.projects) }
            </Table>
          </Col>
        </Row>
      </div>
    );
  }
}