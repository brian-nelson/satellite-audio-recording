import BaseDao from "./BaseDao.js"

export default class SceneDao extends BaseDao {
  getScenes(projectId) {
    return this.read(`/projects/${projectId}/scenes`);
  }
}